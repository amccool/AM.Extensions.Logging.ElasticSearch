using AM.Extensions.Logging.ElasticSearch;
using ElasticLogger.Test.Entities;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ElasticLogger.Test
{
    public class SerializerTests
    {
        private readonly ITestOutputHelper _output;

        public SerializerTests(ITestOutputHelper outputHelper)
        {
            _output = outputHelper;
        }

        [Fact]
        public async Task TryingToBreakThisThing()
        {
            Circle sillyCircle = new Circle{me = null};
            sillyCircle.me = sillyCircle;

            ElasticsearchJsonNetSerializer serializer = new ElasticsearchJsonNetSerializer();

            MemoryStream stream = new MemoryStream();

            serializer.Serialize(sillyCircle, stream);

            var reader = new StreamReader(stream);

            await reader.ReadToEndAsync();
        }
    }
}
