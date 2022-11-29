//using DotNet.Testcontainers.Builders;
//using DotNet.Testcontainers.Configurations;
//using DotNet.Testcontainers.Containers;
//using Elasticsearch.Net;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ElasticLogger.Test.Fixture
//{
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
//}
