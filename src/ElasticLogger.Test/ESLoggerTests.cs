using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AM.Extensions.Logging.ElasticSearch;
using Elasticsearch.Net;
using ElasticsearchInside;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Nest;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ElasticLogger.Test
{
    public class ESLoggerTests
    {
        private readonly ITestOutputHelper _output;

        public ESLoggerTests(ITestOutputHelper outputHelper)
        {
            _output = outputHelper;
        }

        [Fact]
        public async Task FullESWriteTest()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(c => c.SetElasticsearchStartTimeout(60)
                .EnableLogging().LogTo(s=>_output.WriteLine(s??string.Empty))))
            {
                await elasticsearch.Ready();

                ILoggerFactory loggerFactory = new LoggerFactory()
                    .AddElasticSearch(elasticsearch.Url);

                var logger = loggerFactory.CreateLogger("xxxxxxx");

                var circularRefObj = new Circle();
                circularRefObj.me = circularRefObj;

                logger.Log(LogLevel.Critical, new EventId(), circularRefObj, null, (circle, exception) => "");

                var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));
                await client.PingAsync();
                await delayTask;

                var resp = await client.CatIndicesAsync();

                Assert.Single(resp.Records);
            }
        }

        [Fact]
        public async Task FullESTest()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(c => c.SetElasticsearchStartTimeout(60)
                .EnableLogging().LogTo(s => _output.WriteLine(s ?? string.Empty))))
            {
                await elasticsearch.Ready();
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));
                await client.PingAsync();

                var tweet = new Tweet
                {
                    Id = 1,
                    User = "kimchy",
                    PostDate = new DateTime(2009, 11, 15),
                    Message = "Trying out NEST, so far so good?"
                };
                var response = await client.IndexAsync(tweet, idx => idx.Index("mytweetindex"));
                var response2 = client.Get<Tweet>(1, idx => idx.Index("mytweetindex"));
                var tweetResp = response;
                var tweetResp2 = response2.Source;
                Assert.Equal(tweet, tweetResp2);
            }
        }



        [Fact]
        public async Task TryingToBreakThisThing()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { };

            Circle sillyCircle = new Circle{me = null};
            sillyCircle.me = sillyCircle;

            ElasticsearchJsonNetSerializer serializer = new ElasticsearchJsonNetSerializer();

            MemoryStream stream = new MemoryStream();

            serializer.Serialize(sillyCircle, stream);

            var reader = new StreamReader(stream);

            var huh = await reader.ReadToEndAsync();
        }

        [Fact]
        public async Task LoggingThing()
        {
            var factory = new CustomWebApplicationFactory<Startup>();

            var client = factory.CreateClient();

            await client.GetAsync("/");
        }
    }
}
