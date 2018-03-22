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
            string logKey = Guid.NewGuid().ToString();

            //// Push ID to log
            //using (LogContext.PushProperty("LogKey", logKey))
            //{
            //    await _backupService.Run(source);
            //}

            for (int i = 0; i < 1000; i++)
            {
                Breakstuff();
            }

            _logger.LogInformation("Ending Service for  with LogKey {logKey}");
        }

        private void Breakstuff()
        {
            int x = 0;
            try
            {
                var y = 100 / x;
            }
            catch (Exception e)
            {
                _logger.LogError(100, e, "sdgsgsgsg", 1, "3");
            }
        }
    }
}
