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
    public class ElasticsearchLoggerFilterTests : IClassFixture<ESFixture>
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
            var source = "My.Bananas.Trace";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        {"Logging:Elasticsearch:IncludeScopes", "true"},
                        {"Logging:Elasticsearch:LogLevel:Default", "Error"},
                        {"Logging:Elasticsearch:LogLevel:" + source, "Trace"},
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
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogTrace("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            //not 
            //var resp = await client.CatIndicesAsync();
            //Assert.Single(resp.Records);

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                //.Index("trace-*")
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Single(dyndocs.Documents);
        }


        [Fact]
        public async Task configured_categories_dont_log_for_config()
        {
            var source = "Billy.bob";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        {"Logging:Elasticsearch:IncludeScopes", "true"},
                        {"Logging:Elasticsearch:LogLevel:Default", "Error"},
                        {"Logging:Elasticsearch:LogLevel:" + source, "Error"},
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
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogTrace("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            //var resp = await client.CatIndicesAsync();
            //Assert.Empty(resp.Records);

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                //.Index("trace-*")
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Empty(dyndocs.Documents);
        }


        [Fact]
        public async Task No_elasticsearch_section_should_use_default_log_level()
        {
            var source = "bizarro";
            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                            .Add(new MemoryConfigurationSource
                            {
                                InitialData = new Dictionary<string, string>
                                {
                                    {"Logging:LogLevel:Default", "Trace" }
                                }
                            })
                            .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(x =>
                {
                    x.AddConfiguration(config.GetSection("Logging"));
                    x.AddElasticSearch(options =>
                    {
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogTrace("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Single(dyndocs.Documents);

        }


        [Fact]
        public async Task Write_critical_log_with_no_config()
        {
            var source = "xxxxxxx";
            await _fixture.ReadyAsync();

            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddElasticSearch(_fixture.Endpoint);

            var logger = loggerFactory.CreateLogger(source);

            var circularRefObj = new Circle();
            circularRefObj.me = circularRefObj;

            logger.Log(Microsoft.Extensions.Logging.LogLevel.Critical, new EventId(), circularRefObj, null, (circle, exception) => "");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            //var resp = await client.CatIndicesAsync();
            //Assert.Single(resp.Records);

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Single(dyndocs.Documents);
        }




        [Fact]
        public async Task Load_ES_with_explicit_type_write_and_search()
        {
            await _fixture.ReadyAsync();
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();

            var tweet = new Tweet
            {
                Id = 1,
                User = "kimchy",
                PostDate = new DateTime(2009, 11, 15),
                Message = "Trying out NEST, so far so good?"
            };
            var response = await client.IndexAsync(tweet, idx => idx.Index("mytweetindex"));
            var response2 = client.Get<Tweet>(1, idx => idx.Index("mytweetindex"));
            var tweetResp = response;
            var tweetResp2 = response2.Source;
            Assert.Equal(tweet, tweetResp2);

            await Task.Delay(TimeSpan.FromSeconds(5));

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .Index("mytweetindex")
                .AllTypes());

            Assert.Single(dyndocs.Documents);
        }

        [Fact]
        public async Task Missing_ElasticSearch_Section_In_Config_Should_Use_Logging_Defaults_Negative_Match()
        {
            var source = "I.Need.A.New.Source";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
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
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogTrace("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Empty(dyndocs.Documents);
        }

        [Fact]
        public async Task Missing_ElasticSearch_Section_In_Config_Should_Use_Logging_Defaults_Positive_Match()
        {
            var source = "My.Apples.Trace";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
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
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogError("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Single(dyndocs.Documents);
        }

        [Fact]
        public async Task Existing_ElasticSearch_Section_In_Config_Should_Be_Used_For_Log_Levels_Positive_Match()
        {
            var source = "Tangerine.Peel";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        {"Logging:LogLevel:Default", "Information"},
                        {"Logging:Elasticsearch:LogLevel:Default", "Error"}
                    }
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(x =>
                {
                    x.AddConfiguration(config.GetSection("Logging"));
                    x.AddElasticSearch(options =>
                    {
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogTrace("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Empty(dyndocs.Documents);
        }

        [Fact]
        public async Task Existing_ElasticSearch_Section_In_Config_Should_Be_Used_For_Log_Levels_Negative_Match()
        {
            var source = "Tangerine.Seeds";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        {"Logging:LogLevel:Default", "Information"},
                        {"Logging:Elasticsearch:LogLevel:Default", "Error"}
                    }
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(x =>
                {
                    x.AddConfiguration(config.GetSection("Logging"));
                    x.AddElasticSearch(options =>
                    {
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogCritical("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Single(dyndocs.Documents);
        }

        [Fact]
        public async Task Existing_ElasticSearch_Section_With_Category_Should_Match_Top_Level_Positive_Match()
        {
            var source = "Grapes.Peel";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        {"Logging:LogLevel:Default", "Information"},
                        {"Logging:Elasticsearch:LogLevel:Default", "Error"},
                        {"Logging:Elasticsearch:LogLevel:Grapes", "Information"},
                    }
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(x =>
                {
                    x.AddConfiguration(config.GetSection("Logging"));
                    x.AddElasticSearch(options =>
                    {
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogInformation("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Single(dyndocs.Documents);
        }

        [Fact]
        public async Task Existing_ElasticSearch_Section_With_Category_Should_Match_Top_Level_Negative_Match()
        {
            var source = "Apples.Peel";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        {"Logging:LogLevel:Default", "Information"},
                        {"Logging:Elasticsearch:LogLevel:Default", "Error"},
                        {"Logging:Elasticsearch:LogLevel:Grapes", "Information"},
                    }
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(x =>
                {
                    x.AddConfiguration(config.GetSection("Logging"));
                    x.AddElasticSearch(options =>
                    {
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogInformation("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Empty(dyndocs.Documents);
        }

        [Fact]
        public async Task Existing_ElasticSearch_Section_With_Category_Should_Match_Bottom_Level_Positive_Match()
        {
            var source = "Pears.Peel";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        {"Logging:LogLevel:Default", "Information"},
                        {"Logging:Elasticsearch:LogLevel:Default", "Error"},
                        {"Logging:Elasticsearch:LogLevel:Grapes.Peel", "Error"},
                    }
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(x =>
                {
                    x.AddConfiguration(config.GetSection("Logging"));
                    x.AddElasticSearch(options =>
                    {
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogCritical("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Single(dyndocs.Documents);
        }

        [Fact]
        public async Task Existing_ElasticSearch_Section_With_Category_Should_Match_Bottom_Level_Negative_Match()
        {
            var source = "Pears.Peel";

            await _fixture.ReadyAsync();

            var config = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        {"Logging:LogLevel:Default", "Information"},
                        {"Logging:Elasticsearch:LogLevel:Default", "Error"},
                        {"Logging:Elasticsearch:LogLevel:Grapes.Peel", "Error"},
                    }
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(x =>
                {
                    x.AddConfiguration(config.GetSection("Logging"));
                    x.AddElasticSearch(options =>
                    {
                        options.ElasticsearchEndpoint = _fixture.Endpoint;
                        options.IndexName = "trace";
                    });
                })
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(source);

            logger.LogInformation("bananas taste yucky");

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5));
            var client = new ElasticClient(new ConnectionSettings(_fixture.Endpoint));
            await client.PingAsync();
            await delayTask;

            var dyndocs = await client.SearchAsync<dynamic>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q
                    .Match(m => m
                        .Field("Source")
                        .Query(source)
                    ))
            );

            Assert.Single(dyndocs.Documents);
        }
    }
}
