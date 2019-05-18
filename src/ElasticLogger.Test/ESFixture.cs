using System;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace ElasticLogger.Test
{
    public class ESFixture : IDisposable
    {
        //private ITestOutputHelper _output;
        private readonly ElasticsearchInside.Elasticsearch _elasticsearch;

        public Uri Endpoint => _elasticsearch.Url;

        public ESFixture()
        {
            //_output = output;

            _elasticsearch = new ElasticsearchInside.Elasticsearch(c => c.SetElasticsearchStartTimeout(60)
                .EnableLogging());
                //.LogTo(s => _output.WriteLine(s ?? string.Empty)));

            _elasticsearch.ReadySync();
        }

        public Task<ElasticsearchInside.Elasticsearch> ReadyAsync()
        {
            return _elasticsearch.Ready();
        }

        public void Dispose()
        {
            _elasticsearch?.Dispose();
        }
    }
}