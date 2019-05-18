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
using Newtonsoft.Json.Linq;
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
