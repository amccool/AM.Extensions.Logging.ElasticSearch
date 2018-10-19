using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.Extensions.Logging
{

    internal class ElasticsearchLoggerOptionsSetup : ConfigureFromConfigurationOptions<ElasticsearchLoggerOptions>
    {
        public ElasticsearchLoggerOptionsSetup(ILoggerProviderConfiguration<ElasticsearchLoggerProvider> providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}
