using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using System.Threading.Tasks;
using Xunit;

namespace ElasticLogger.Test
{
    internal class ContainerESTests
    {
        ContainerESTests()
        {
        }

        public async Task DoTheTest()
        {
            await new TestcontainersBuilder<ElasticsearchTestcontainer>()
      .WithDatabase(new ElasticsearchTestcontainerConfiguration())
      .Build()
      .StartAsync()
      .ConfigureAwait(false);
        }
    }


    //[Collection(nameof(Testcontainers))]
    public sealed class ElasticsearchTestcontainerTest : IClassFixture<ElasticsearchFixture>
    {
        private readonly ElasticsearchFixture elasticsearchFixture;

        public ElasticsearchTestcontainerTest(ElasticsearchFixture elasticsearchFixture)
        {
            this.elasticsearchFixture = elasticsearchFixture;
        }

        [Fact]
        [Trait("Category", "Elasticsearch")]
        public async Task ConnectionEstablished()
        {
            // Given
            var connection = this.elasticsearchFixture.Connection;

            // When
            var result = await connection.InfoAsync()
              .ConfigureAwait(false);

            // Then
            Assert.True(result.IsValid);
        }
    }
