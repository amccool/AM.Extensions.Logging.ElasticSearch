using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.Extensions.Logging
{
    public class ElasticsearchLoggerOptions
    {
        public Uri ElasticsearchEndpoint { get; set; }
        public string IndexName { get; set; } = "trace";
    }
}
