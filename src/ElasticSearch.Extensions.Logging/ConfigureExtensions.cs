using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElasticSearch.Extensions.Logging
{
    public static class ConfigureExtensions
    {
        /// <summary>
        /// Enable ElasticSearch as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="endpoint">uri with ES port 9200 endpoint</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint)
        {
            return AddElasticSearch(factory, endpoint, (n, l) => l >= LogLevel.Trace, "trace");
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




        /// <summary>
        /// Adds a ElasticSearch logger named 'ElasticSearch' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">elasticsearch uri for the cluster at port 9200</param>
        public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder, Uri endpoint)
        {
            return builder.AddElasticSearch(endpoint, (n, l) => l >= LogLevel.Trace, "trace");
        }


        /// <summary>
        /// Adds a ElasticSearch logger named 'ElasticSearch' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">elasticsearch uri for the cluster at port 9200</param>
        /// <param name="filter">the filter for this logger</param>
        public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder, Uri endpoint, Func<string, LogLevel, bool> filter)
        {
            return builder.AddElasticSearch(endpoint, filter, "trace");
        }


        /// <summary>
        /// Adds a console logger named 'Console' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">elasticsearch uri for the cluster at port 9200</param>
        /// <param name="filter">the filter for this logger</param>
        /// <param name="indexPrefix">the prefix for the trace to be logged to, -YYYY-MM-DD-HH will be added</param>
        public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder, Uri endpoint, Func<string, LogLevel, bool> filter, string indexPrefix = "trace")
        {
            builder.Services.AddSingleton<ILoggerProvider, ElasticSearchLoggerProvider>(provider =>
            {
                return new ElasticSearchLoggerProvider(endpoint, filter, indexPrefix);
            });
            return builder;
        }
    }
}
