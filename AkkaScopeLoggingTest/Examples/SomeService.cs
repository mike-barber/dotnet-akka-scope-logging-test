using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AkkaScopeLoggingTest.Examples
{
    public class SomeService
    {
        readonly ILogger<SomeService> _logger;

        public SomeService(ILogger<SomeService> logger)
        {
            _logger = logger;
        }

        public async Task MyAsyncMethod()
        {
            _logger.LogInformation("Starting my async method");
            await Task.Delay(1500);
            _logger.LogInformation("Finishing my async method");
        }
    }
}