using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AkkaScopeLoggingTest.AkkaExtensions
{
    public interface IActorSystemService : IDisposable
    {
        ActorSystem ActorSystem { get; }
    }

    public class ActorSystemService : IActorSystemService, IDisposable
    {
        readonly ActorSystem _actorSystem;
        readonly ILogger<ActorSystemService> _logger;

        public ActorSystemService(IServiceProvider serviceProvider, string actorSystemName)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<ActorSystemService>>();

            // create the actor system, using serilog as the akka logger for internal stuff
            _actorSystem = ActorSystem.Create(actorSystemName,
                "akka { loglevel=INFO,  loggers=[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]}");

            // register our extension
            _actorSystem.RegisterExtension(new MicrosoftAbstractionsExtensionProvider(serviceProvider));

            _logger.LogInformation("Created actor system: {actorSystem}", _actorSystem);
        }

        public ActorSystem ActorSystem { get => _actorSystem; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (_actorSystem != null)
            {
                _actorSystem.Terminate().GetAwaiter().GetResult();
                _actorSystem.Dispose();
            }
            _logger.LogInformation("Actor system terminated and disposed.");
        }
    }
}