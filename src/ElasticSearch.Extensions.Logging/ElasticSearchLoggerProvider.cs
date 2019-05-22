using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
        private IElasticLowLevelClient _client;
        private readonly Uri _endpoint;
        private readonly string _indexPrefix;
        private readonly BlockingCollection<JObject> _queueToBePosted = new BlockingCollection<JObject>();

        private const string DocumentType = "doc";
        private Action<JObject> _scribeProcessor;

        #endregion



        /// <summary>
        /// prefix for the Index for traces
        /// </summary>
        private string Index => this._indexPrefix.ToLower() + "-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH");


        public ElasticsearchLoggerProvider(IOptions<ElasticsearchLoggerOptions> options) : this(options.Value)
        { }

        public ElasticsearchLoggerProvider(ElasticsearchLoggerOptions options)
        {
            _endpoint = options.ElasticsearchEndpoint;
            _indexPrefix = options.IndexName;

            //build the client
            //build the batcher
            Initialize();

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
                    var singleNode = new SingleNodeConnectionPool(_endpoint);

                    var cc = new ConnectionConfiguration(singleNode,
                            connectionSettings => new ElasticsearchJsonNetSerializer())
                        .EnableHttpPipelining()
                        .ThrowExceptions();

                    //the 1.x serializer we needed to use, as the default SimpleJson didnt work right
                    //Elasticsearch.Net.JsonNet.ElasticsearchJsonNetSerializer()

                    this._client = new ElasticLowLevelClient(cc);
                    return this._client;
                }
            }
        }

        private void Initialize()
        {
            //setup a flag in config to chose
            //SetupObserver();
            SetupObserverBatchy();
        }


        private void SetupObserver()
        {
            _scribeProcessor = a => WriteDirectlyToES(a);

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

        private async Task WriteDirectlyToESAsBatch(IEnumerable<JObject> jos)
        {
            if (!jos.Any())
                return;

            var indx = new { index = new { _index = Index, _type = DocumentType } };
            var indxC = Enumerable.Repeat(indx, jos.Count());

            var bb = jos.Zip(indxC, (f, s) => new object[] { s, f });
            var bbo = bb.SelectMany(a => a);

            try
            {
                await Client.BulkPutAsync<VoidResponse>(Index, DocumentType, bbo.ToArray(), br => br.Refresh(false));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void WriteToQueueForProcessing(JObject jo)
        {
            this._queueToBePosted.Add(jo);
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
