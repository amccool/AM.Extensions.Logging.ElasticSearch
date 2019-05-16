using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AM.Extensions.Logging.ElasticSearch
{
    [ProviderAlias("Elasticsearch")]
    public class ElasticsearchLoggerProvider : ILoggerProvider
    {
        #region fields
        //private readonly Func<string, LogLevel, bool> _filter;
        private readonly string _indexPrefix;
        private readonly Uri _endpoint;
        //private readonly IOptions<LoggerFilterOptions> _filterOptions;
        #endregion


        public ElasticsearchLoggerProvider(IOptions<ElasticsearchLoggerOptions> options) : this(options.Value)
        { }

        public ElasticsearchLoggerProvider(ElasticsearchLoggerOptions options)
        {
            _endpoint = options.ElasticsearchEndpoint;
            _indexPrefix = options.IndexName;
        }




        //public ElasticsearchLoggerProvider(IOptionsMonitor<ElasticsearchLoggerOptions> options, IOptions<LoggerFilterOptions> filterOptions)
        //{
        //    _indexPrefix = options.CurrentValue.IndexName;
        //    _endpoint = options.CurrentValue.ElasticsearchEndpoint;

        //    _filterOptions = filterOptions;

        //    //// Filter would be applied on LoggerFactory level
        //    //_filter = trueFilter;
        //    //_optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        //    //ReloadLoggerOptions(options.CurrentValue);
        //}

        public ILogger CreateLogger(string categoryName)
        {
            //var logLevel = GetLogLevelForCategoryName(categoryName);

            return new ElasticsearchLogger(categoryName, _endpoint, _indexPrefix);//, logLevel);
        }

        //private LogLevel GetLogLevelForCategoryName(string categoryName)
        //{
        //    if (_filterOptions == null || _filterOptions.Value == null || _filterOptions.Value.Rules == null)
        //        return LogLevel.Warning;

        //    var providerSpecific = _filterOptions.Value.Rules
        //        .Where(x=>!string.IsNullOrEmpty(x.ProviderName))
        //        .Where(x => x.ProviderName.Equals("Elasticsearch"))
        //        .Where(x =>!string.IsNullOrEmpty(x.CategoryName));
        //    if (providerSpecific.Any())
        //    {
        //        var matched = providerSpecific.FirstOrDefault(x => categoryName.Contains(x.CategoryName));
        //        return matched?.LogLevel ?? _filterOptions.Value.MinLevel;
        //    }
        //    else
        //    {
        //        return _filterOptions.Value.MinLevel;
        //    }
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
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
