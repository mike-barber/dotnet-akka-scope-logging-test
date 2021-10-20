using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AkkaScopeLoggingTest.AkkaExtensions
{
    public class MicrosoftAbstractionsExtension : IExtension
    {
        public ILoggerFactory LoggerFactory { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }

        public MicrosoftAbstractionsExtension(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            ServiceProvider = serviceProvider;
        }
    }

    public class MicrosoftAbstractionsExtensionProvider : ExtensionIdProvider<MicrosoftAbstractionsExtension>
    {
        readonly MicrosoftAbstractionsExtension _extensionInstance;

        public MicrosoftAbstractionsExtensionProvider(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            _extensionInstance = new MicrosoftAbstractionsExtension(
                serviceProvider,
                loggerFactory
            );
        }

        public override MicrosoftAbstractionsExtension CreateExtension(ExtendedActorSystem system)
            => _extensionInstance;
    }

    public static class MicrosoftAbstractionsExtensionHelpers
    {
        // Get an untyped ILogger, and extract both class name AND path from 
        // the context. The normal typed ILogger<T> just uses T to derive the 
        // class name.
        public static ILogger GetILogger(this IUntypedActorContext context)
        {
            var loggerFactory = context.System.GetExtension<MicrosoftAbstractionsExtension>().LoggerFactory;
            var type = context.Props.Type;

            var typeName = type.FullName;
            var actorPath = context.Self.Path.ToString();
            var categoryName = $"{typeName}({actorPath})";

            return loggerFactory.CreateLogger(categoryName);
        }


        public static IServiceProvider GetServiceProvider(this IUntypedActorContext context)
        {
            return context.System.GetExtension<MicrosoftAbstractionsExtension>().ServiceProvider;
        }
    }
}