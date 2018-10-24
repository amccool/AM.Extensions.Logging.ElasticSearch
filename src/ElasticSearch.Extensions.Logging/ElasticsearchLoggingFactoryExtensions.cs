using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
using Elasticsearch.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elasticsearch.Extensions.Logging
{
    public static class ElasticsearchLoggingFactoryExtensions
    {

        //#region logger factory
        ///// <summary>
        ///// Enable ElasticSearch as logging provider in .NET Core.
        ///// </summary>
        ///// <param name="factory"></param>
        ///// <param name="endpoint">uri with ES port 9200 endpoint</param>
        ///// <returns>ILoggerFactory for chaining</returns>
        //public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint)
        //{
        //    return AddElasticSearch(factory, endpoint, LogLevel.Trace, "trace");
        //}
        //public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint, LogLevel minLevel)
        //{
        //    return AddElasticSearch(factory, endpoint, (n, l) => l >= minLevel, "trace");
        //}

        //public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint, LogLevel filter, string indexPrefix = "trace")
        //{
        //    factory.AddProvider(new ElasticsearchLoggerProvider(endpoint, filter, indexPrefix));
        //    return factory;
        //}
        //#endregion

    }
}
