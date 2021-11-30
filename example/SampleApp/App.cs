using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SampleApp
{
    public class App : IApp
    {
        //private readonly IBackupService _backupService;
        private readonly ILogger<App> _logger;
        //private readonly IConfigurationRoot _config;

        //public App(IBackupService backupService, IConfigurationRoot config, ILogger<App> logger)
        //{
        //    _backupService = backupService;
        //    _logger = logger;
        //    _config = config;
        //}
        public App(ILogger<App> logger)
        {
            _logger = logger;
        }

        public async Task Run()
        {
            _logger.LogCritical("Run...................");
            _logger.LogDebug("Run...................");
            _logger.LogError("Run...................");
            _logger.LogInformation("Run...................");
            _logger.LogTrace("Run...................");
            _logger.LogWarning("Run...................");

            string logKey = Guid.NewGuid().ToString();

            //// Push ID to log
            //using (LogContext.PushProperty("LogKey", logKey))
            //{
            //    await _backupService.Run(source);
            //}

            for (int i = 0; i < 5000; i++)
            {
                await Breakstuff();
                _logger.LogCritical("Run...................");
                _logger.LogDebug("Run...................");
                _logger.LogError("Run...................");
                _logger.LogInformation("Run...................");
                _logger.LogTrace("Run...................");
                _logger.LogWarning("Run...................");
            }

            _logger.LogInformation("Ending Service for  with LogKey {logKey}", logKey);
        }

        private Task Breakstuff()
        {
            int x = 0;
            try
            {
                var y = 100 / x;
            }
            catch (Exception e)
            {
                _logger.LogError(100, e, "sdgsgsgsg {a} {b}", 1, "sdfsdgfsg");
            }
            return Task.CompletedTask;
        }
    }
}
