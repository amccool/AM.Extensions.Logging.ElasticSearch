using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace ElasticLogger.Test
{
    public class ContainerESTests
    {
        [Fact]
        public async Task DoTheTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(b => {
                b.AddDebug();
                b.AddConsole(); 
            });

            var secureHttpServiceProvider = serviceCollection.BuildServiceProvider();
            var loggerFactory = secureHttpServiceProvider.GetService<ILoggerFactory>();

            ILogger logger = loggerFactory.CreateLogger("es");

            TestcontainersSettings.Logger = logger;


            /**
             * WARNING
             * "org.elasticsearch.plugins.PluginsService","elasticsearch.node.name":"1b1454ac59a8","elasticsearch.cluster.name":"docker-cluster"}
             * bootstrap check failure [1] of [1]: max virtual memory areas vm.max_map_count [65530] is too low, increase to at least [262144]
             * ERROR: Elasticsearch did not exit normally - check the logs at /usr/share/elasticsearch/logs/docker-cluster.log
             * 
             * ERROR: [1] bootstrap checks failed. You must address the points described in the following [1] lines before starting Elasticsearch.
             * 
             * 
             * 
             * in docker-desktop
             * https://gist.github.com/jarek-przygodzki/4721932bba3bd434512ae6cc58f508b0
             * wsl -d docker-desktop
             * echo "vm.max_map_count = 262144" > /etc/sysctl.d/99-docker-desktop.conf
             * reboot
             * 
             * basically the linux VM running docker needs
             * wsl -d docker-desktop
             * sysctl -w vm.max_map_count=262144
             * 
             * 
             **/

            var container = new TestcontainersBuilder<ElasticsearchTestcontainer>()
              .WithDatabase(new ElasticsearchTestcontainerConfiguration()
              { 
               Password="aaaa",
                
              })
              //.WithAutoRemove(false)
              //.WithWaitStrategy( Wait.ForUnixContainer())
              //.WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
              .Build();

            await container.StartAsync()
                .ConfigureAwait(false);

            Assert.NotEmpty(container.ConnectionString);
        }
    }


    ////[Collection(nameof(Testcontainers))]
    //public sealed class ElasticsearchTestcontainerTest : IClassFixture<ElasticsearchFixture>
    //{
    //    private readonly ElasticsearchFixture elasticsearchFixture;

    //    public ElasticsearchTestcontainerTest(ElasticsearchFixture elasticsearchFixture)
    //    {
    //        this.elasticsearchFixture = elasticsearchFixture;
    //    }

    //    [Fact]
    //    [Trait("Category", "Elasticsearch")]
    //    public async Task ConnectionEstablished()
    //    {
    //        // Given
    //        var connection = this.elasticsearchFixture.Connection;

    //        // When
    //        var result = await connection.InfoAsync()
    //          .ConfigureAwait(false);

    //        // Then
    //        Assert.True(result.IsValid);
    //    }
    //}
}
