using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AM.Extensions.Logging.ElasticSearch
{
    public static class ElasticsearchLoggingFactoryExtensions
    {

        #region logger factory
        /// <summary>
        /// Enable ElasticSearch as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="endpoint">uri with ES port 9200 endpoint</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint)
        {
            return AddElasticSearch(factory, new ElasticsearchLoggerOptions()
            {
                ElasticsearchEndpoint = endpoint,
                IndexName = "trace"
            });
        }
        public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint, string indexPrefix)
        {
            return AddElasticSearch(factory, new ElasticsearchLoggerOptions()
            {
                ElasticsearchEndpoint = endpoint,
                IndexName = indexPrefix
            });
        }

        public static ILoggerFactory AddElasticSearch(this ILoggerFactory loggerFactory, ElasticsearchLoggerOptions options)
        {
            loggerFactory.AddProvider(new ElasticsearchLoggerProvider(options));
            return loggerFactory;
        }

        #endregion

    }
}
