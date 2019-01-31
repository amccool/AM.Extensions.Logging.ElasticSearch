using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace AM.Extensions.Logging.ElasticSearch
{

    internal class ElasticsearchLoggerOptionsSetup : ConfigureFromConfigurationOptions<ElasticsearchLoggerOptions>
    {
        public ElasticsearchLoggerOptionsSetup(ILoggerProviderConfiguration<ElasticsearchLoggerProvider> providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}
