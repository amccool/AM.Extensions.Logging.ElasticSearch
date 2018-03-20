using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ElasticSearch.Extensions.Logging
{
    public static class ConfigureExtensions
    {
        /// <summary>
        /// Enable ElasticSearch as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="endpoint">ES port 9200 endpoint</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint)
        {
            return AddElasticSearch(factory, endpoint, (n, l) => l >= LogLevel.Information, "trace");
        }
        public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint, LogLevel minLevel)
        {
            return AddElasticSearch(factory, endpoint, (n, l) => l >= minLevel, "trace");
        }

        public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint, Func<string, LogLevel, bool> filter, string indexPrefix = "trace")
        {
            factory.AddProvider(new ElasticSearchLoggerProvider(endpoint, filter, indexPrefix));
            return factory;

        }
    }
}
