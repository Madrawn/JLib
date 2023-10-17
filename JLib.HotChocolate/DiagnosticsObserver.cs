using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DiagnosticAdapter;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution;
using HotChocolate.Execution.Processing;
using Microsoft.Extensions.Logging;
using Serilog;

namespace JLib.HotChocolate;
public class LoggingEventListener : ExecutionDiagnosticEventListener
{
    private readonly ILogger<LoggingEventListener> _logger;

    public LoggingEventListener(ILogger<LoggingEventListener> logger)
        => _logger = logger;

    public override IDisposable ExecuteRequest(IRequestContext context)
    {
        _logger.LogDebug("Executing Request");
        _logger.LogTrace("request data: {requestData}", context);
        var disposable = base.ExecuteRequest(context);
        _logger.LogTrace("request completed");
        return disposable;
    }

    public override IDisposable ExecuteOperation(IRequestContext context)
    {
        _logger.LogDebug("Executing Operation");
        _logger.LogTrace("request data: {requestData}", context);
        var disposable = base.ExecuteOperation(context);
        _logger.LogTrace("request completed");
        return disposable;
    }

    public override IDisposable ExecuteSubscription(ISubscription subscription)
    {
        _logger.LogDebug("Executing Subscription");
        _logger.LogTrace("request data: {requestData}", subscription);
        var disposable = base.ExecuteSubscription(subscription);
        _logger.LogTrace("request completed");
        return disposable;
    }

    public override void RequestError(IRequestContext context,
        Exception exception)
    {
        _logger.LogError(exception, "A request error occurred!: {e}", exception.Message);
    }
}
