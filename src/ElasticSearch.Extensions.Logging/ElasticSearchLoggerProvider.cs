using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace AM.Extensions.Logging.ElasticSearch
{
    [ProviderAlias("Elasticsearch")]
    public class ElasticsearchLoggerProvider : ILoggerProvider
    {
        #region fields
        private readonly IOptionsMonitor<ElasticsearchLoggerOptions> _optionsMonitor;
        private IElasticLowLevelClient _client;
        private readonly BlockingCollection<JObject> _queueToBePosted = new BlockingCollection<JObject>();
        private const string DocumentType = "doc";
        private Action<JObject> _scribeProcessor;
        #endregion

        /// <summary>
        /// prefix for the Index for traces
        /// </summary>
        private string Index => _optionsMonitor.CurrentValue.IndexName.ToLower() + "-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH");


        public ElasticsearchLoggerProvider(IOptionsMonitor<ElasticsearchLoggerOptions> optionsMonitor)
        {
            if(optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }
            _optionsMonitor = optionsMonitor;

            _optionsMonitor.OnChange(UpdateClientWithNewOptions);

            Initialize();
        }

        private void UpdateClientWithNewOptions(ElasticsearchLoggerOptions newOptions)
        {
            var newClient = CreateNewElasticLowLevelClient(newOptions.ElasticsearchEndpoint);

            _client = newClient;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ElasticsearchLogger(categoryName, _scribeProcessor);
        }

        public IElasticLowLevelClient Client
        {
            get
            {
                if (_client != null)
                {
                    return _client;
                }
                else
                {
                    this._client = CreateNewElasticLowLevelClient(_optionsMonitor.CurrentValue.ElasticsearchEndpoint);

                    return this._client;
                }
            }
        }

        private ElasticLowLevelClient CreateNewElasticLowLevelClient(Uri elasticSearchEndpoint)
        {
            var singleNode = new SingleNodeConnectionPool(_optionsMonitor.CurrentValue.ElasticsearchEndpoint);

            var cc = new ConnectionConfiguration(singleNode, new ElasticsearchJsonNetSerializer())
                .ServerCertificateValidationCallback((obj, cert, chain, policyerrors) => true)                      //notice we allowing invalid certs!!!!!   this should get a warning, and configuration
            .EnableHttpPipelining()
            .EnableHttpCompression()
            .ThrowExceptions();

            return new ElasticLowLevelClient(cc);
        }

        private void Initialize()
        {
            //TODO: setup a flag in config to chose
            //SetupObserver();
            SetupObserverBatchy();
        }


        private void SetupObserver()
        {
            _scribeProcessor = async (a) => await WriteDirectlyToES(a);

            //this._queueToBePosted.GetConsumingEnumerable()
            //.ToObservable(Scheduler.Default)
            //.Subscribe(x => WriteDirectlyToES(x));
        }


        private void SetupObserverBatchy()
        {
            _scribeProcessor = a => WriteToQueueForProcessing(a);

            this._queueToBePosted.GetConsumingEnumerable()
                .ToObservable(Scheduler.Default)
                .Buffer(TimeSpan.FromSeconds(1), 10)
                .Subscribe(async x => await this.WriteDirectlyToESAsBatch(x));
        }



        private async Task WriteDirectlyToES(JObject jo)
        {
            try
            {
                await Client.IndexAsync<VoidResponse>(Index, DocumentType, jo.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        //POST /_bulk? filter_path = items.*.error
        private static Dictionary<string, object> filter_path = new Dictionary<string, object>() { { "filter_path", "items.*.error" } };

        private Task WriteDirectlyToESAsBatch(IEnumerable<JObject> jos)
        {
            if (!jos.Any())
                return Task.CompletedTask;

            var indx = new { index = new { _index = Index, _type = DocumentType } };
            var indxC = Enumerable.Repeat(indx, jos.Count());

            var bb = jos.Zip(indxC, (f, s) => new object[] { s, f });
            var bbo = bb.SelectMany(a => a);

            _ = Client.BulkPutAsync<VoidResponse>(Index, DocumentType,
                PostData.MultiJson(bbo.ToArray()),
                new BulkRequestParameters { Refresh = Refresh.False,  QueryString=filter_path })
                .ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        Debug.WriteLine(x.Exception);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);

            return Task.CompletedTask;
        }

        private void WriteToQueueForProcessing(JObject jo)
        {
            _queueToBePosted.Add(jo);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //_client.
                    _queueToBePosted.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ElasticSearchLoggerProvider() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
