This is a very light-touch approach to using the Microsoft ILogger abstraction consistently in 
normal code and within Akka.net actors.

- We supply the ServiceProvider via a simple akka Extension
- We're not using the akka Serilog adapter in our actors
    - because we want to use ILogger, not Serilog, for consistency
    - we do still let Akka use its Serilog adapter for internal purposes
- However, in order to create a logger scoped for the actor, we need to:
    - Get an instance of the Serilog logger, and use ForContext to wrap it with the ActorPath
    - Wrap this in a SerilogProvider, and use this to produce a standard dotnet ILogger
    - It is not sufficient using BeginScope to add the actor path, as the scope lives for a far shorter time than the actor -- it's bound to the thread or async context
- We're not using the akka DI for asp.net core; it's unnecessarily complex for this
- We're using Serilog expression templates for nice formatting with optional fields
- Helper methods for
    - Getting the ILogger for an actor from its context
    - Getting the ServiceProvider for an actor from its context
    - Example of building a scope for a CorrelationId
