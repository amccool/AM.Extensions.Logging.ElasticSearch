using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AM.Extensions.Logging.ElasticSearch
{
    public class ElasticsearchLogger : ILogger
    {

        private readonly LogLevel _logLevel;
        private readonly string _userDomainName;
        private readonly string _userName;
        private readonly string _machineName;
        private readonly Action<JObject> _scribeProcessor;

        public ElasticsearchLogger(string categoryName, Action<JObject> scribeProcessor)
        {
            Name = categoryName;
            _scribeProcessor = scribeProcessor;

            // Default is to turn on all the logging
            _logLevel = LogLevel.Trace;

            _userDomainName = Environment.UserDomainName;
            _userName = Environment.UserName;
            _machineName = Environment.MachineName;

        }

        //public ElasticsearchLogger(string categoryName, Uri endpoint, string indexPrefix)
        //{
        //    Name = categoryName;
            
        //    _endpoint = endpoint;
        //    _indexPrefix = indexPrefix;

        //    // Default is to turn on all the logging
        //    _logLevel = LogLevel.Trace;

        //    _userDomainName = Environment.UserDomainName;
        //    _userName = Environment.UserName;
        //    _machineName = Environment.MachineName;
        //    Initialize();
        //}

        public string Name { get; }
        


        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && logLevel >= _logLevel;
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
            string loggerName,
            LogLevel eventType,
            int id,
            string message,
            Guid? relatedActivityId,
            object data)
        {
            string updatedMessage = message;
            JObject payload = null;
            var serializerIgnoreReferenceLoop = new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            if (data != null)
            {
                if (data is Exception)
                {
                    updatedMessage = ((Exception)data).Message;

                    
                    payload = JObject.FromObject(data, serializerIgnoreReferenceLoop);
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

                    string json = JsonConvert.SerializeObject(xmlContent, new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
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
                        payload = JObject.FromObject(data, serializerIgnoreReferenceLoop);
                    }
                    catch (JsonSerializationException jEx)
                    {
                        payload = new JObject();
                        payload.Add("FAILURE", jEx.Message);
                        payload.Add("data", data.GetType().ToString());
                    }
                }
            }

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




    }
}
