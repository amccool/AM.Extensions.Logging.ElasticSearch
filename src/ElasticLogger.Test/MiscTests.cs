using System.IO;
using System.Threading.Tasks;
using AM.Extensions.Logging.ElasticSearch;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace ElasticLogger.Test
{
    public class MiscTests
    {
        private readonly ITestOutputHelper _output;

        public MiscTests(ITestOutputHelper outputHelper)
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

        [Fact(Skip ="issues with hosting")]
        public async Task LoggingThing()
        {
            var factory = new CustomWebApplicationFactory<Startup>();

            var client = factory.CreateClient();

            await client.GetAsync("/");
        }
    }
}
