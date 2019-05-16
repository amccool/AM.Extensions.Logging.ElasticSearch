using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;

namespace AM.Extensions.Logging.ElasticSearch
{
    public static class ElasticsearchLoggerFilterOptionsExtensions
    {
        public static LogLevel GetLogLevelForCategoryName(this IOptions<LoggerFilterOptions> filterOptions, string categoryName)
        {
            if (filterOptions == null || filterOptions.Value == null || filterOptions.Value.Rules == null)
                return LogLevel.Warning;

            var elasticSearchOptions = filterOptions.Value.Rules
                .Where(x => !string.IsNullOrEmpty(x.ProviderName))
                .Where(x => x.ProviderName.Equals("Elasticsearch"));

            var foundLogLevel = elasticSearchOptions
                .Where(x => x.CategoryName == categoryName)
                .FirstOrDefault();

            var defaultSettings = elasticSearchOptions
                .Where(x => x.CategoryName == null)
                .FirstOrDefault();

            var retval = foundLogLevel?.LogLevel ?? (defaultSettings?.LogLevel ?? filterOptions.Value.MinLevel);

            return retval;
        }
    }
}
