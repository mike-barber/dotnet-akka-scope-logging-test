using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AkkaSerilogTest
{
    public static class LoggerScopeHelper
    {
        // BeginScope understands IEnumerable<KeyValuePair<string,object>> to map to properties; everything else won't map correctly.
        // refer to: https://nblumhardt.com/2016/11/ilogger-beginscope/
        public static IDisposable BeginScopeProperty(this ILogger logger, string name, object value)
            => logger.BeginScope(new[] { KeyValuePair.Create(name, value) });

        public static IDisposable BeginScopeMethod(this ILogger logger, [System.Runtime.CompilerServices.CallerMemberName] string callerMethodName = null)
            => BeginScopeProperty(logger, "Method", callerMethodName);

        public static IDisposable BeginScopeCorrelationId(this ILogger logger, string correlationId)
            => BeginScopeProperty(logger, "CorrelationId", correlationId);
    }
}
