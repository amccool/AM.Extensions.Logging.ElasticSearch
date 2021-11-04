using System;
using System.IO;
using System.Threading.Tasks;
using AM.Extensions.Logging.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SampleApp
{
    class Program
    {
        //private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public Program()
        {
            var loggingConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("logging.json", optional: false, reloadOnChange: true)
                .Build();

            // A Web App based program would configure logging via the WebHostBuilder.
            // Create a logger factory with filters that can be applied across all logger providers.
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IApp, App>()
                .AddLogging(builder =>
                {
                    builder
                        .AddConfiguration(loggingConfiguration.GetSection("Logging"))
                        //.AddFilter("Microsoft", LogLevel.Warning)
                        //.AddFilter("System", LogLevel.Warning)
                        //.AddFilter("SampleApp.Program", LogLevel.Debug)
                        .AddConsole(options =>
                        {
                            options.DisableColors = true;
                            options.IncludeScopes = true;
                        })
                        .AddEventSourceLogger()
                        .AddElasticSearch(options =>
                        {
                            //options.ElasticsearchEndpoint = new Uri(@"http://localhost:9200/");
                            //options.ElasticsearchEndpoint = new Uri(@"https://elasticsearch.mgmc.rauland/");
                            options.ElasticsearchEndpoint = new Uri(@"http://es.devint.dev-r5ead.net:9200/");
                            //options.IndexName = "trace";
                        });
                });

            // providers may be added to a LoggerFactory before any loggers are created 


            _serviceProvider = serviceCollection.BuildServiceProvider();
            // getting the logger using the class's name is conventional
            //_logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        }

        public static void Main(string[] args)
        {
            new Program().Runner(args);
        }


        public void Runner(string[] args)
        {
            Console.WriteLine("Hello World!");

            //// Create service collection
            //var serviceCollection = new ServiceCollection();
            //var serviceProvider = ConfigureServices(serviceCollection);

            var logger = _serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();


            logger.LogCritical("Starting application");
            logger.LogError("Starting application");
            logger.LogDebug("Starting application");
            logger.LogInformation("Starting application");
            logger.LogTrace("Starting application");
            logger.LogWarning("Starting application");


            //do the actual work here
            var bar = _serviceProvider.GetService<IApp>();

            Task.Run(() => bar.Run()).Wait();

            logger.LogCritical("All done!");
            logger.LogError("All done!");
            logger.LogDebug("All done!");
            logger.LogInformation("All done!");
            logger.LogTrace("All done!");
            logger.LogWarning("All done!");

            Console.ReadLine();
        }

    }
}
