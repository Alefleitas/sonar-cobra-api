using Microsoft.Extensions.Configuration;
using nordelta.cobra.webapi.Services.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services
{
    public class HelperService : IHelperService
    {
        private readonly IConfiguration _configuration;

        public HelperService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task Healthy()
        {
            var expiredDeletionInterval = _configuration.GetSection("KeepAlive").Value;

            Log.Information(expiredDeletionInterval);
            return Task.CompletedTask;
        }
    }
}
