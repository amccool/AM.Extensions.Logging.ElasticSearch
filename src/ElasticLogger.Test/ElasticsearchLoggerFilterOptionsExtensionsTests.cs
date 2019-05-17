using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AM.Extensions.Logging.ElasticSearch;
using System;
using Xunit;
using Microsoft.Extensions.Configuration.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using Xunit.Abstractions;

namespace ElasticLogger.Test
{
    public class ElasticsearchLoggerFilterTests: IClassFixture<ESFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly ESFixture _fixture;

        public ElasticsearchLoggerFilterTests(ESFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }
        



        [Fact]
        public async Task No_categories_for_es_should_use_default()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(c => c.SetElasticsearchStartTimeout(60)
                .EnableLogging().LogTo(s => _output.WriteLine(s ?? string.Empty))))
            {
                await elasticsearch.Ready();

                var config = new ConfigurationBuilder()
                    .Add(new MemoryConfigurationSource
                    {
                        InitialData = new Dictionary<string, string>
                        {
                            {"Logging:Elasticsearch:IncludeScopes", "true"},
                            {"Logging:Elasticsearch:LogLevel:Default", "Error"},
                            {"Logging:Elasticsearch:LogLevel:My.Bananas.Trace", "Trace"},
                            {"Logging:LogLevel:Default", "Information"}
                        }
                    })
                    .Build();

                var serviceProvider = new ServiceCollection()
                    .AddLogging(x =>
                    {
                        x.AddConfiguration(config.GetSection("Logging"));
                        x.AddElasticSearch(options =>
                        {
                            options.ElasticsearchEndpoint = elasticsearch.Url;
                            options.IndexName = "trace";
                        });
                    })
                    .BuildServiceProvider();

                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("My.Bananas.Trace");

                logger.LogTrace("bananas taste yucky");

                var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));
                await client.PingAsync();
                await delayTask;

                var resp = await client.CatIndicesAsync();

                Assert.Single(resp.Records);
            }
        }


        [Fact]
        public async Task configured_categories_dont_log_for_config()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(c => c.SetElasticsearchStartTimeout(60)
                .EnableLogging().LogTo(s => _output.WriteLine(s ?? string.Empty))))
            {
                await elasticsearch.Ready();

                var config = new ConfigurationBuilder()
                    .Add(new MemoryConfigurationSource
                    {
                        InitialData = new Dictionary<string, string>
                        {
                            {"Logging:Elasticsearch:IncludeScopes", "true"},
                            {"Logging:Elasticsearch:LogLevel:Default", "Error"},
                            {"Logging:Elasticsearch:LogLevel:My.Bananas.Trace", "Error"},
                            {"Logging:LogLevel:Default", "Error"}
                        }
                    })
                    .Build();

                var serviceProvider = new ServiceCollection()
                    .AddLogging(x =>
                    {
                        x.AddConfiguration(config.GetSection("Logging"));
                        x.AddElasticSearch(options =>
                        {
                            options.ElasticsearchEndpoint = elasticsearch.Url;
                            options.IndexName = "trace";
                        });
                    })
                    .BuildServiceProvider();

                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("My.Bananas.Trace");

                logger.LogTrace("bananas taste yucky");

                var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));
                await client.PingAsync();
                await delayTask;

                var resp = await client.CatIndicesAsync();

                Assert.Empty(resp.Records);
            }
        }


















        [Fact]
        public async Task No_elasticsearch_section_should_use_default_log_level()
        {
            var config = new ConfigurationBuilder()
                            .Add(new MemoryConfigurationSource
                            {
                                InitialData = new Dictionary<string, string>
                                {
                                    {"Logging:LogLevel:Default", "Trace" }
                                }
                            })
                            .Build();


            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(c => c.SetElasticsearchStartTimeout(60)
                .EnableLogging().LogTo(s => _output.WriteLine(s ?? string.Empty))))
            {
                await elasticsearch.Ready();


                var serviceProvider = new ServiceCollection()
                    .AddLogging(x =>
                    {
                        x.AddConfiguration(config.GetSection("Logging"));
                        x.AddElasticSearch(options =>
                        {
                            options.ElasticsearchEndpoint = elasticsearch.Url;
                            options.IndexName = "trace";
                        });
                    })
                    .BuildServiceProvider();

                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("My.Bananas.Trace");

                logger.LogTrace("bananas taste yucky");

                var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));
                await client.PingAsync();
                await delayTask;

                var resp = await client.CatIndicesAsync();

                Assert.Single(resp.Records);

                var inm = new IndexName();
                inm.Name = "trace-*";
                var i = Indices.Index(inm);

                var indxResponse = await client.GetIndexAsync(Indices.Index("trace-*"));

                Assert.Single(indxResponse.Indices);

                //s=>s.Index(Indices.AllIndices).

                var tester = client.SearchAsync<dynamic>(s => s.From(0).Size(1)
                    .Query(q => q
                        .Terms(t => t
                            .Name("named_query")
                            .Boost(1.1f)
                            .Field("cvrNummer")
                            .Terms("36406208"))));


                var docs = await client.SearchAsync<dynamic>(s => s
                    .AllIndices()
                    .From(0)
                    .Size(100));
                //.Query(q=>q.Match(m=>m.Field(f=>f)))

                Assert.Single(docs.Documents);


            }
        }

    }
}
