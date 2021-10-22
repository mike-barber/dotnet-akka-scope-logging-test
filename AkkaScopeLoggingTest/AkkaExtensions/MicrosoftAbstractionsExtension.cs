using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AkkaScopeLoggingTest.AkkaExtensions
{
    public class MicrosoftAbstractionsExtension : IExtension
    {
        public IServiceProvider ServiceProvider { get; private set; }

        internal Serilog.ILogger BaseSerilogLogger { get; private set; }

        public MicrosoftAbstractionsExtension(IServiceProvider serviceProvider, Serilog.ILogger baseSerilogLogger)
        {
            ServiceProvider = serviceProvider;
            BaseSerilogLogger = baseSerilogLogger;
        }
    }

    public class MicrosoftAbstractionsExtensionProvider : ExtensionIdProvider<MicrosoftAbstractionsExtension>
    {
        readonly MicrosoftAbstractionsExtension _extensionInstance;

        public MicrosoftAbstractionsExtensionProvider(IServiceProvider serviceProvider)
        {
            // Get the DI provided serilog logger, if available. This will be the case when 
            // the serilog asp.net extensions have been configured as per instructions 
            // with `CreateBootstrapLogger()`. Refer to: https://github.com/serilog/serilog-aspnetcore 
            var baseSerilogLogger = serviceProvider.GetService<Serilog.ILogger>();

            // Fall back on the static global logger if the extensions have been configured
            // the simpler way using Microsoft's ILoggingBuilder.AddSerilog() instead.
            if (baseSerilogLogger == null)
            {
                baseSerilogLogger = Serilog.Log.Logger;
                baseSerilogLogger.Information("Extension using the global Serilog logger");
            }

            _extensionInstance = new MicrosoftAbstractionsExtension(
                serviceProvider,
                baseSerilogLogger
            );
        }

        public override MicrosoftAbstractionsExtension CreateExtension(ExtendedActorSystem system)
            => _extensionInstance;
    }

    public static class MicrosoftAbstractionsExtensionHelpers
    {
        /// Get an ILogger for this actor, using the actor's type as the SourceContext
        /// and pushing the ActorPath into its context. This delivers behaviour
        /// consistent with the normal Akka Serilog logger.
        ///
        /// Note that we have to drop into Serilog to use ForContext, as the dotnet abstractions 
        /// do not expose the this interface. `BeginScope` on an ILogger just pushes properties
        /// onto the current loger context, which don't live long enough for the lifetime of 
        /// the actor. We have to create a specific context for each actor.
        public static ILogger GetILogger(this IUntypedActorContext context)
        {
            var extension = context.System.GetExtension<MicrosoftAbstractionsExtension>();

            var typeName = context.Props.Type.FullName;
            var actorPath = context.Self.Path.ToString();

            // - use the base logger that was injected into our extension - note, not 
            //   just the static logger, as it's not necessarily the right one.
            // - create the context containing the ActorPath
            // - create a serilog extensions provider wrapping this serilog logger
            // - use this provider to create a dotnet ILogger, which we return
            var serilogLogger = extension.BaseSerilogLogger.ForContext("ActorPath", actorPath);
            var provider = new Serilog.Extensions.Logging.SerilogLoggerProvider(serilogLogger);
            var logger = provider.CreateLogger(typeName);

            return logger;
        }

        public static IServiceProvider GetServiceProvider(this IUntypedActorContext context)
        {
            return context.System.GetExtension<MicrosoftAbstractionsExtension>().ServiceProvider;
        }
    }
}