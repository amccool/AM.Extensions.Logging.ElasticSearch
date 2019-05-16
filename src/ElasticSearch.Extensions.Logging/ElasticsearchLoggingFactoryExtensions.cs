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
        //public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint, LogLevel minLevel)
        //{
        //    return AddElasticSearch(factory, endpoint, (n, l) => l >= minLevel, "trace");
        //}

        //public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint, LogLevel filter, string indexPrefix)
        //{
        //    var x = new OptionsMonitor< ElasticsearchLoggerOptions >(new OptionsFactory<ElasticsearchLoggerOptions>(), )



        //    var o = new ElasticsearchLoggerOptions()
        //    {
        //        ElasticsearchEndpoint = endpoint,
        //        IndexName = indexPrefix
        //    };
        //    factory.AddProvider(new ElasticsearchLoggerProvider(, 
        //        endpoint, filter, indexPrefix));
        //    return factory;
        //}


        public static ILoggerFactory AddElasticSearch(this ILoggerFactory loggerFactory, ElasticsearchLoggerOptions options)
        {
            loggerFactory.AddProvider(new ElasticsearchLoggerProvider(options));
            return loggerFactory;
        }

        #endregion

    }
}
