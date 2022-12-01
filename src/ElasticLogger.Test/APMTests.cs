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
            await _fixture.ReadyAsync();

            var factory = new CustomWebApplicationFactory<Startup>(_fixture);

            var client = factory.CreateClient();

            await client.GetAsync("/");
        }
    }
}
