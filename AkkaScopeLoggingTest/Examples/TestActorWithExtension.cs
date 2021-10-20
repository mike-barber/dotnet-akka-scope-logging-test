using Akka.Actor;
using AkkaScopeLoggingTest.AkkaExtensions;
using AkkaSerilogTest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AkkaScopeLoggingTest.Examples
{
    public class TestActorWithExtension : ReceiveActor
    {
        // get logger and some other service from our context helpers
        readonly ILogger _logger = Context.GetILogger();
        readonly SomeService _someService = Context.GetServiceProvider().GetRequiredService<SomeService>();


        public TestActorWithExtension()
        {
            _logger.LogInformation("Created actor");
            ReceiveAsync<Message>(ProcessMessage);
        }

        private async Task ProcessMessage(Message msg)
        {
            using (var scope = _logger.BeginScopeCorrelationId(msg.CorrelationId))
            {
                _logger.LogInformation("Received Message: {@msg}", msg);

                // call another method on the actor
                InternalMethod();

                // call a method on an external service
                await _someService.MyAsyncMethod();

                _logger.LogInformation("Completed Message processing, with message {MessageText}", msg.MessageText);
            }
            _logger.LogInformation("Finished: outside of correlation scope");
        }

        private void InternalMethod()
        {
            using (_logger.BeginScope("Internal Method processing"))
            {
                _logger.LogInformation("Did something");
            }
        }
    }
}
