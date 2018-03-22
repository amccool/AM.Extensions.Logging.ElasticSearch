using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace ElasticSearch.Extensions.Logging
{
    public class ElasticSearchLogger : ILogger
    {
        private IElasticLowLevelClient _client;
        private readonly Uri _endpoint;
        private readonly string _indexPrefix;
        private readonly BlockingCollection<JObject> _queueToBePosted = new BlockingCollection<JObject>();
        private readonly string _userDomainName;
        private readonly string _userName;
        private readonly string _machineName;

        public ElasticSearchLogger(string name, Uri endpoint, Func<string, LogLevel, bool> filter, string indexPrefix)
        {
            Name = name;
            Filter = filter ?? ((category, logLevel) => true);

            _endpoint = endpoint;
            _indexPrefix = indexPrefix;

            _userDomainName = Environment.UserDomainName;
            _userName = Environment.UserName;
            _machineName = Environment.MachineName;
            Initialize();
        }

        /// <summary>
        /// prefix for the Index for traces
        /// </summary>
        private string Index => this._indexPrefix.ToLower() + "-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH");

        public string Name { get; }

        public Func<string, LogLevel, bool> Filter
        {
            get { return _filter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _filter = value;
            }
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
            //SetupObserver();
            SetupObserverBatchy();
        }

        private Action<JObject> _scribeProcessor;
        private Func<string, LogLevel, bool> _filter;

        private void SetupObserver()
        {
            _scribeProcessor = a => WriteDirectlyToES(a);

            //this._queueToBePosted.GetConsumingEnumerable()
            //.ToObservable(Scheduler.Default)
            //.Subscribe(x => WriteDirectlyToES(x));
        }


        private void SetupObserverBatchy()
        {
            _scribeProcessor = a => WriteToQueueForprocessing(a);

            this._queueToBePosted.GetConsumingEnumerable()
                .ToObservable(Scheduler.Default)
                .Buffer(TimeSpan.FromSeconds(1), 10)
                .Subscribe(async x => await this.WriteDirectlyToESAsBatch(x));
        }




        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Filter(Name, logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, 
            TState state, 
            Exception exception, 
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (state == null && exception == null)
            {
                return;
            }

            var message = formatter(state, exception);
            
            WriteTrace(Name, logLevel, eventId.Id, message, Guid.Empty, exception);
        }




        protected void WriteTrace(
            //TraceEventCache eventCache,
            string loggerName,
            LogLevel eventType,
            int id,
            string message,
            Guid? relatedActivityId,
            object data)
        {

            //if (eventCache != null && eventCache.Callstack.Contains(nameof(Elasticsearch.Net.ElasticLowLevelClient)))
            //{
            //    return;
            //}

            string updatedMessage = message;
            JObject payload = null;
            if (data != null)
            {
                if (data is Exception)
                {
                    updatedMessage = ((Exception)data).Message;
                    payload = JObject.FromObject(data);
                }
                else if (data is XPathNavigator)
                {
                    var xdata = data as XPathNavigator;
                    //xdata.MoveToRoot();

                    XDocument xmlDoc;
                    try
                    {
                        xmlDoc = XDocument.Parse(xdata.OuterXml);

                    }
                    catch (Exception)
                    {
                        xmlDoc = XDocument.Parse(xdata.ToString());
                        //eat
                        //throw;
                    }

                    // Convert the XML document in to a dynamic C# object.
                    dynamic xmlContent = new ExpandoObject();
                    ExpandoObjectHelper.Parse(xmlContent, xmlDoc.Root);

                    string json = JsonConvert.SerializeObject(xmlContent);
                    payload = JObject.Parse(json);
                }
                else if (data is DateTime)
                {
                    payload = new JObject();
                    payload.Add("System.DateTime", (DateTime)data);
                }
                else if (data is string)
                {
                    payload = new JObject();
                    payload.Add("string", (string)data);
                }
                else if (data.GetType().IsValueType)
                {
                    payload = new JObject { { "data", data.ToString() } };
                }
                else
                {
                    try
                    {
                        payload = JObject.FromObject(data);
                    }
                    catch (JsonSerializationException jEx)
                    {
                        payload = new JObject();
                        payload.Add("FAILURE", jEx.Message);
                        payload.Add("data", data.GetType().ToString());
                    }
                }
            }

            //Debug.Assert(!string.IsNullOrEmpty(updatedMessage));
            //Debug.Assert(payload != null);

            InternalWrite(new TraceEventCache(), loggerName, eventType, id, updatedMessage, relatedActivityId, payload);
        }





        private void InternalWrite(
                    TraceEventCache eventCache,
                    string loggerName,
                    LogLevel eventType,
                    int? traceId,
                    string message,
                    Guid?
                    relatedActivityId,
                    JObject dataObject)
        {

            //var timeStamp = DateTime.UtcNow.ToString("o");
            //var source = Process.GetCurrentProcess().ProcessName;
            //var stacktrace = Environment.StackTrace;
            //var methodName = (new StackTrace()).GetFrame(StackTrace.METHODS_TO_SKIP + 4).GetMethod().Name;


            DateTime logTime;
            string logicalOperationStack = null;
            if (eventCache != null)
            {
                logTime = eventCache.DateTime.ToUniversalTime();
                if (eventCache.LogicalOperationStack != null && eventCache.LogicalOperationStack.Count > 0)
                {
                    StringBuilder stackBuilder = new StringBuilder();
                    foreach (object o in eventCache.LogicalOperationStack)
                    {
                        if (stackBuilder.Length > 0) stackBuilder.Append(", ");
                        stackBuilder.Append(o);
                    }
                    logicalOperationStack = stackBuilder.ToString();
                }
            }
            else
            {
                logTime = DateTimeOffset.UtcNow.UtcDateTime;
            }

            string threadId = eventCache != null ? eventCache.ThreadId : string.Empty;
            string thread = Thread.CurrentThread.Name ?? threadId;

            IPrincipal principal = Thread.CurrentPrincipal;
            IIdentity identity = principal?.Identity;
            string identityname = identity == null ? string.Empty : identity.Name;

            string username = $"{_userDomainName}\\{_userName}";

            try
            {
                var jo = new JObject
                    {
                        {"Source", loggerName },
                        {"TraceId", traceId ?? 0},
                        {"EventType", eventType.ToString()},
                        {"UtcDateTime", logTime},
                        {"timestamp", eventCache?.Timestamp ?? 0},
                        {"MachineName", _machineName},
                        {"AppDomainFriendlyName", AppDomain.CurrentDomain.FriendlyName},
                        {"ProcessId", eventCache?.ProcessId ?? 0},
                        {"ThreadName", thread},
                        {"ThreadId", threadId},
                        {"Message", message},
                        {"ActivityId", Trace.CorrelationManager.ActivityId != Guid.Empty ? Trace.CorrelationManager.ActivityId.ToString() : string.Empty},
                        {"RelatedActivityId", relatedActivityId.HasValue ? relatedActivityId.Value.ToString() : string.Empty},
                        {"LogicalOperationStack", logicalOperationStack},
                        {"Data", dataObject},
                        {"Username", username},
                        {"Identityname", identityname},
                    };

                _scribeProcessor(jo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }



        private async Task WriteDirectlyToES(JObject jo)
        {
            try
            {
                await Client.IndexAsync<VoidResponse>(Index, "_doc", jo.ToString());
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

            var indx = new { index = new { _index = Index, _type = "_doc" } };
            var indxC = Enumerable.Repeat(indx, jos.Count());

            var bb = jos.Zip(indxC, (f, s) => new object[] { s, f });
            var bbo = bb.SelectMany(a => a);

            try
            {
                await Client.BulkPutAsync<VoidResponse>(Index, "_doc", bbo.ToArray(), br => br.Refresh(false));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void WriteToQueueForprocessing(JObject jo)
        {
            this._queueToBePosted.Add(jo);
        }


    }
}
