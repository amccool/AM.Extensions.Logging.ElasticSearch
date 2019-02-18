using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AM.Extensions.Logging.ElasticSearch;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElasticLogger.Test
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup: class
    {
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
                Uri uri = new Uri(@"http://es.fakething.net:9200");

                logging.AddElasticSearch(options => { options.ElasticsearchEndpoint = uri; });
            });

            base.ConfigureWebHost(builder);
        }
    }
}
