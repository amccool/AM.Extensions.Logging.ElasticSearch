using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AM.Extensions.Logging.ElasticSearch;
using System.Net;
using System;
using Microsoft.Extensions.Options;
using Xunit;
using Microsoft.Extensions.Configuration.Memory;
using System.Collections.Generic;

namespace ElasticLogger.Test
{
    public class ElasticsearchLoggerFilterOptionsExtensionsTests
    {
        [Fact]
        public void No_categories_for_es_should_use_default()
        {
            var config = new ConfigurationBuilder()
                            .Add(new MemoryConfigurationSource
                            {
                                InitialData = new Dictionary<string, string>
                                {
                                    {"Logging:Elasticsearch:IncludeScopes", "true" },
                                    {"Logging:Elasticsearch:LogLevel:Default", "Error" },
                                    {"Logging:Elasticsearch:LogLevel:My.Bananas.Trace", "Trace" },
                                    {"Logging:LogLevel:Default", "Information" }
                                }
                            })
                            .Build();

            var serviceProvider = new ServiceCollection()
                                        .AddLogging(x =>
                                        {
                                            x.AddConfiguration(config.GetSection("Logging"));
                                            x.AddElasticSearch(options =>
                                            {
                                                options.ElasticsearchEndpoint = new Uri("http://bananas.net");
                                                options.IndexName = "trace";
                                            });
                                        })
                                        .BuildServiceProvider();

            var loggerFilterOptions = serviceProvider.GetService<IOptions<LoggerFilterOptions>>();

            //This category doesn't exist in our diagnostics.json, so we should get the default (error)
            var retrievedLogLevel = loggerFilterOptions.GetLogLevelForCategoryName("logLevelThatDoesntExist");

            Assert.Equal(LogLevel.Error, retrievedLogLevel);
        }

        [Fact]
        public void Should_use_log_level_of_matching_category_name()
        {
            var config = new ConfigurationBuilder()
                            .Add(new MemoryConfigurationSource
                            {
                                InitialData = new Dictionary<string, string>
                                {
                                    {"Logging:Elasticsearch:IncludeScopes", "true" },
                                    {"Logging:Elasticsearch:LogLevel:Default", "Error" },
                                    {"Logging:Elasticsearch:LogLevel:My.Bananas.Trace", "Trace" },
                                    {"Logging:LogLevel:Default", "Information" }
                                }
                            })
                            .Build();

            var serviceProvider = new ServiceCollection()
                                        .AddLogging(x =>
                                        {
                                            x.AddConfiguration(config.GetSection("Logging"));
                                            x.AddElasticSearch(options =>
                                            {
                                                options.ElasticsearchEndpoint = new Uri("http://bananas.net");
                                                options.IndexName = "trace";
                                            });
                                        })
                                        .BuildServiceProvider();

            var loggerFilterOptions = serviceProvider.GetService<IOptions<LoggerFilterOptions>>();

            //This category doesn't exist in our diagnostics.json, so we should get the default (error)
            var retrievedLogLevel = loggerFilterOptions.GetLogLevelForCategoryName("My.Bananas.Trace");

            Assert.Equal(LogLevel.Trace, retrievedLogLevel);
        }

        [Fact]
        public void No_elasticsearch_section_should_use_default_log_level()
        {
            var config = new ConfigurationBuilder()
                            .Add(new MemoryConfigurationSource
                            {
                                InitialData = new Dictionary<string, string>
                                {
                                    {"Logging:LogLevel:Default", "Information" }
                                }
                            })
                            .Build();

            var serviceProvider = new ServiceCollection()
                                        .AddLogging(x =>
                                        {
                                            x.AddConfiguration(config.GetSection("Logging"));
                                            x.AddElasticSearch(options =>
                                            {
                                                options.ElasticsearchEndpoint = new Uri("http://bananas.net");
                                                options.IndexName = "trace";
                                            });
                                        })
                                        .BuildServiceProvider();

            var loggerFilterOptions = serviceProvider.GetService<IOptions<LoggerFilterOptions>>();

            //This category doesn't exist in our diagnostics.json, so we should get the default (error)
            var retrievedLogLevel = loggerFilterOptions.GetLogLevelForCategoryName("My.Bananas.Trace");

            Assert.Equal(LogLevel.Information, retrievedLogLevel);
        }

        [Fact]
        public void No_logging_config_should_use_builders_minimum_level()
        {
            var serviceProvider = new ServiceCollection()
                                        .AddLogging(x =>
                                        {
                                            x.SetMinimumLevel(LogLevel.Critical);
                                            x.AddElasticSearch(options =>
                                            {
                                                options.ElasticsearchEndpoint = new Uri("http://bananas.net");
                                                options.IndexName = "trace";
                                            });
                                        })
                                        .BuildServiceProvider();

            var loggerFilterOptions = serviceProvider.GetService<IOptions<LoggerFilterOptions>>();

            //This category doesn't exist in our diagnostics.json, so we should get the default (error)
            var retrievedLogLevel = loggerFilterOptions.GetLogLevelForCategoryName("My.Bananas.Trace");

            Assert.Equal(LogLevel.Critical, retrievedLogLevel);
        }
    }
}
