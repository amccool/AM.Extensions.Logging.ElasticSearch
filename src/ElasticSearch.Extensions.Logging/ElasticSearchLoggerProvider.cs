using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ElasticSearch.Extensions.Logging
{
    public class ElasticSearchLoggerProvider : ILoggerProvider
    {
        #region fields
        private readonly Func<string, LogLevel, bool> _filter;

        private readonly string _indexPrefix;
        private readonly Uri _endpoint;
        #endregion

        public ElasticSearchLoggerProvider(Uri endpoint, Func<string, LogLevel, bool> filter, string indexPrefix)
        {
            this._endpoint = endpoint;
            this._filter = filter;
            this._indexPrefix = indexPrefix;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ElasticSearchLogger(categoryName, _endpoint, _filter, _indexPrefix);
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
