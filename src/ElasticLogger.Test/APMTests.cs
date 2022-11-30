using ElasticLogger.Test.Factory;
using ElasticLogger.Test.Fixture;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ElasticLogger.Test
{
    public class APMTests : IClassFixture<ElasticsearchFixture>
    {
        private readonly ElasticsearchFixture _fixture;
        private readonly ITestOutputHelper _output;

        public APMTests(ElasticsearchFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _output = outputHelper;
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
