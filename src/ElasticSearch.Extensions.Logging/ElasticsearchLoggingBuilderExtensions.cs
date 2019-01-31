using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace AM.Extensions.Logging.ElasticSearch
{
    public static class ElasticsearchLoggingBuilderExtensions
    {
        /// <summary>
        /// Adds a Elasticsearch logger named 'Elasticsearch' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        private static ILoggingBuilder AddElasticSearch(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ElasticsearchLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ElasticsearchLoggerOptions>, ElasticsearchLoggerOptionsSetup>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<ElasticsearchLoggerOptions>, LoggerProviderOptionsChangeTokenSource<ElasticsearchLoggerOptions, ElasticsearchLoggerProvider>>());
            return builder;
        }

        /// <summary>
        /// Adds a Elasticsearch logger named 'Elasticsearch' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure">action to configure the needed parameters of the logger itself</param>
        /// <returns></returns>
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
