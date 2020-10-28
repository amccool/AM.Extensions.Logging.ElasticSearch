using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ElasticLogger.Test
{
    public class ESFixture : IAsyncLifetime, IDisposable
    {
        //private ITestOutputHelper _output;
        private readonly ElasticsearchInside.Elasticsearch _elasticsearch;

        public Uri Endpoint => _elasticsearch.Url;

        private readonly IMessageSink _messageSink;

        public ESFixture(IMessageSink messageSink)
        {
            _messageSink = messageSink;

            _elasticsearch = new ElasticsearchInside.Elasticsearch(c => c.SetElasticsearchStartTimeout(60)
                .EnableLogging()
                .LogTo(s => _messageSink.OnMessage(new Xunit.Sdk.DiagnosticMessage(s ?? string.Empty)))
                );

            _elasticsearch.ReadySync();
        }

        public Task<ElasticsearchInside.Elasticsearch> ReadyAsync()
        {
            return _elasticsearch.Ready();
        }

        public void Dispose()
        {
            //uber importadante !
            _elasticsearch?.Dispose();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}