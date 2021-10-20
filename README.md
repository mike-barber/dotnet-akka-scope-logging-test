This is a very light-touch approach to using the Microsoft ILogger abstraction consistently in 
normal code and within Akka.net actors.

- We supply the ServiceProvider and LoggerFactory via a simple akka Extension
- We're not using the akka Serilog adapter in our actors
    - because we want to use ILogger, not Serilog, for consistency
    - we do still let Akka use its Serilog adapter for internal purposes
- We're not using the akka DI for asp.net core; it's unnecessarily complex for this
- We're using Serilog expression templates for nice formatting with optional fields
- Helper methods for
    - Getting the ILogger for an actor from its context
    - Getting the ServiceProvider for an actor from its context
    - Example of building a scope for a CorrelationId
