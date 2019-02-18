using System;

namespace AM.Extensions.Logging.ElasticSearch
{
    public class ElasticsearchLoggerOptions
    {
        public Uri ElasticsearchEndpoint { get; set; }
        public string IndexName { get; set; } = "trace";
    }
}
