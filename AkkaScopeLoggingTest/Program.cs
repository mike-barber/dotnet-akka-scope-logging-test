using System;
using Akka.Actor;
using AkkaScopeLoggingTest.AkkaExtensions;
using AkkaScopeLoggingTest.Examples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AkkaScopeLoggingTest
{
    class Program : IDisposable
    {
        readonly ServiceProvider _serviceProvider;
        readonly ILogger<Program> _logger;

        private Program()
        {
            // Refer to Serilog Expressions: https://github.com/serilog/serilog-expressions
            // expression template that echoes out scopes
            var expressionTemplateWithScope = new Serilog.Templates.ExpressionTemplate(
                "{@t:HH:mm:ss.fff} [{@l}] {SourceContext}  " +
                "{#if ActorPath is not null}{ActorPath} {#end}" +
                "{#if CorrelationId is not null}CorrelationId={CorrelationId} {#end}" +
                "{@m} " +
                "{#each s in Scope} => {s}{#delimit}{#end}" +
                "\n{@x}",
                theme: Serilog.Templates.Themes.TemplateTheme.Code,
                applyThemeWhenOutputIsRedirected: false);

            // setup serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(expressionTemplateWithScope)
                .MinimumLevel.Debug()
                .CreateLogger();

            // create little DI service provider and configure logging
            _serviceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddSerilog();
                })
                .AddSingleton<SomeService>()
                .AddSingleton<IActorSystemService>(sp => new ActorSystemService(
                    sp, 
                    "my-actor-system"
                ))
                .BuildServiceProvider();

            _logger = _serviceProvider.GetRequiredService<ILogger<Program>>();
        }

        private void Run()
        {
            _logger.LogInformation("Program running");

            var actorSystem = _serviceProvider.GetRequiredService<IActorSystemService>().ActorSystem;
            
            var testActor1 = actorSystem.ActorOf(Props.Create(() => new TestActorWithExtension()), "test-actor");
            testActor1.Tell(new Message(Guid.NewGuid().ToString(), "this is a test message"));

            Console.WriteLine("Hit enter to quit");
            Console.ReadLine();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _serviceProvider.Dispose();
        }

        static void Main()
        {
            using var program = new Program();
            program.Run();
        }

        
    }
}
