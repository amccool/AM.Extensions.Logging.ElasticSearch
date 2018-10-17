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
    public static class ConfigureExtensions
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
        //    return AddElasticSearch(factory, endpoint, (n, l) => l >= LogLevel.Trace, "trace");
        //}
        //public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint, LogLevel minLevel)
        //{
        //    return AddElasticSearch(factory, endpoint, (n, l) => l >= minLevel, "trace");
        //}

        //public static ILoggerFactory AddElasticSearch(this ILoggerFactory factory, Uri endpoint, Func<string, LogLevel, bool> filter, string indexPrefix = "trace")
        //{
        //    factory.AddProvider(new ElasticsearchLoggerProvider(endpoint, filter, indexPrefix));
        //    return factory;
        //}
        //#endregion




        /// <summary>
        /// Adds a console logger named 'Console' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        //public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder)
        //{
        //    builder.AddConfiguration();

        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ConsoleLoggerProvider>());
        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ConsoleLoggerOptions>, ConsoleLoggerOptionsSetup>());
        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<ConsoleLoggerOptions>, LoggerProviderOptionsChangeTokenSource<ConsoleLoggerOptions, ConsoleLoggerProvider>>());
        //    return builder;
        //}

        ///// <summary>
        ///// Adds a console logger named 'Console' to the factory.
        ///// </summary>
        ///// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        ///// <param name="configure"></param>
        //public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder, Action<ConsoleLoggerOptions> configure)
        //{
        //    if (configure == null)
        //    {
        //        throw new ArgumentNullException(nameof(configure));
        //    }

        //    builder.AddConsole();
        //    builder.Services.Configure(configure);

        //    return builder;
        //}





        /// <summary>
        /// Adds a ElasticSearch logger named 'ElasticSearch' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">elasticsearch uri for the cluster at port 9200</param>
        //public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder, Uri endpoint, string indexPrefix = "trace")
        //{
        //    return builder.AddElasticSearch(endpoint, (n, l) => l >= LogLevel.Trace, "trace");
        //}


        /// <summary>
        /// Adds a ElasticSearch logger named 'ElasticSearch' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">elasticsearch uri for the cluster at port 9200</param>
        /// <param name="filter">the filter for this logger</param>
        //public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder, Uri endpoint, Func<string, LogLevel, bool> filter)
        //{
        //    return builder.AddElasticSearch(endpoint, filter, "trace");
        //}


        /// <summary>
        /// Adds a console logger named 'Console' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">elasticsearch uri for the cluster at port 9200</param>
        /// <param name="filter">the filter for this logger</param>
        /// <param name="indexPrefix">the prefix for the trace to be logged to, -YYYY-MM-DD-HH will be added</param>
        //public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder, Uri endpoint, Func<string, LogLevel, bool> filter, string indexPrefix = "trace")
        //{
        //    builder.AddConfiguration();

        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ElasticsearchLoggerProvider>());


        //    //builder.Services.AddSingleton<ILoggerProvider, ElasticsearchLoggerProvider>(provider =>
        //    //{
        //    //    return new ElasticsearchLoggerProvider(endpoint, filter, indexPrefix);
        //    //});
        //    return builder;
        //}

        /// <summary>
        /// Adds a console logger named 'Console' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">elasticsearch uri for the cluster at port 9200</param>
        /// <param name="filter">the filter for this logger</param>
        /// <param name="indexPrefix">the prefix for the trace to be logged to, -YYYY-MM-DD-HH will be added</param>
        //public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder, Uri endpoint, string indexPrefix = "trace")
        //{
        //    builder.AddConfiguration();

        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ElasticsearchLoggerProvider>());


        //    builder.Services.AddSingleton<ILoggerProvider, ElasticsearchLoggerProvider>(provider =>
        //    {
        //        return new ElasticsearchLoggerProvider(endpoint, filter, indexPrefix);
        //    });
        //    return builder;
        //}

        /// <summary>
        /// Adds a console logger named 'Console' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ElasticsearchLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ElasticsearchLoggerOptions>, ElasticsearchLoggerOptionsSetup>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<ElasticsearchLoggerOptions>, LoggerProviderOptionsChangeTokenSource<ElasticsearchLoggerOptions, ElasticsearchLoggerProvider>>());
            return builder;
        }


        public static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder, Action<ElasticsearchLoggerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddElasticSearch();
            builder.Services.Configure(configure);

            return builder;
        }


    }
}
