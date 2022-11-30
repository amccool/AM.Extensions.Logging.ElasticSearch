//using DotNet.Testcontainers.Builders;
//using DotNet.Testcontainers.Configurations;
//using DotNet.Testcontainers.Containers;
//using Elasticsearch.Net;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ElasticLogger.Test.Fixture
{
    public sealed class ElasticsearchFixture
    {
        private readonly ElasticsearchTestcontainer _container;
        private readonly Task _startupTask;

//        public Uri Endpoint => new Uri(_container.ConnectionString);
        public Uri Endpoint => new Uri($"http://{_container.Hostname}:{_container.Port}");

        public ElasticsearchFixture()
        {
            ////additional logging (temporary)
            //var serviceCollection = new ServiceCollection();
            //serviceCollection.AddLogging(b =>
            //{
            //    b.AddDebug();
            //    //b.AddConsole();
            //});

            //var secureHttpServiceProvider = serviceCollection.BuildServiceProvider();
            //var loggerFactory = secureHttpServiceProvider.GetService<ILoggerFactory>();

            //ILogger logger = loggerFactory.CreateLogger("es");
            //TestcontainersSettings.Logger = logger;

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

            _container = new TestcontainersBuilder<ElasticsearchTestcontainer>()
              .WithDatabase(new ElasticsearchTestcontainerConfiguration()
              {
                  Password = string.Empty
                  // 
              })
              //.WithImage("elasticsearch:7.17.7")                            //does not expose a https enpoint
              .WithImage("elasticsearch:6.8.23")                             //does not expose a https enpoint
              .WithEnvironment("discovery.type", "single-node")
              //.WithCleanUp(false)
              //.WithAutoRemove(false)
              //.WithWaitStrategy( Wait.ForUnixContainer())
              //.WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
              .Build();

            //do not await the startup
            //save the task for checking if ready yet
            _startupTask = _container.StartAsync();
        }

        public async Task ReadyAsync()
        {
            await _startupTask;
        }


    }






    //    [UsedImplicitly]
    //    public sealed class ElasticsearchFixture : DatabaseFixture<ElasticsearchTestcontainer, ElasticsearchClient>
    //    {
    //        private readonly TestcontainerDatabaseConfiguration configuration = new ElasticsearchTestcontainerConfiguration { Password = "secret" };

    //        public ElasticsearchFixture()
    //        {
    //            this.Container = new TestcontainersBuilder<ElasticsearchTestcontainer>()
    //              .WithDatabase(this.configuration)
    //              .Build();
    //        }

    //        public override async Task InitializeAsync()
    //        {
    //            await this.Container.StartAsync()
    //              .ConfigureAwait(false);

    //            var settings = new ElasticsearchClientSettings(new Uri(this.Container.ConnectionString))
    //              .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
    //              .Authentication(new BasicAuthentication(this.Container.Username, this.Container.Password));

    //            this.Connection = new ElasticsearchClient(settings);
    //        }

    //        public override async Task DisposeAsync()
    //        {
    //            await this.Container.DisposeAsync()
    //              .ConfigureAwait(false);
    //        }

    //        public override void Dispose()
    //        {
    //            this.configuration.Dispose();
    //        }
    //    }
}
