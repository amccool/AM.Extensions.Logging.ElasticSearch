using AM.Extensions.Logging.ElasticSearch;
using ElasticLogger.Test.Fixture;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ElasticLogger.Test.Factory
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private ElasticsearchFixture _fixture;

        public CustomWebApplicationFactory(ElasticsearchFixture fixture) : base()
        {
            _fixture = fixture;
        }


        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .UseStartup<TStartup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(".");

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("diagnostics.json");
            });

            builder.ConfigureLogging((hostingContext, logging) =>
            {
                //Uri uri = new Uri(@"http://es.fakething.net:9200");
                Uri uri = _fixture.Endpoint;

                logging.AddElasticSearch(options => { options.ElasticsearchEndpoint = uri; });
            });

            base.ConfigureWebHost(builder);
        }
    }
}
