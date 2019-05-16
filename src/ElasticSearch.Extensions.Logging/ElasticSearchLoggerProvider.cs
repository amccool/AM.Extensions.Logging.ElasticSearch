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
        private readonly string _indexPrefix;
        private readonly Uri _endpoint;
        #endregion

        public ElasticsearchLoggerProvider(IOptions<ElasticsearchLoggerOptions> options) : this(options.Value)
        { }

        public ElasticsearchLoggerProvider(ElasticsearchLoggerOptions options)
        {
            _endpoint = options.ElasticsearchEndpoint;
            _indexPrefix = options.IndexName;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ElasticsearchLogger(categoryName, _endpoint, _indexPrefix);
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
