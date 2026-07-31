#nullable disable
using Serilog.Core;
using Serilog.Events;

namespace SimpleLauncher.Tests;

internal sealed class NoOpLogger : ILogger
{
    public void Write(LogEvent logEvent)
    {
    }

    public ILogger ForContext(ILogEventEnricher enricher)
    {
        return this;
    }

    public ILogger ForContext(IEnumerable<ILogEventEnricher> enrichers)
    {
        return this;
    }

    public ILogger ForContext(string propertyName, object value, bool destructureObjects = false)
    {
        return this;
    }

    public bool BindMessageTemplate(string messageTemplate, object[] propertyValues,
        out MessageTemplate parsedTemplate, out IEnumerable<LogEventProperty> boundProperties)
    {
        parsedTemplate = null;
        boundProperties = null;
        return false;
    }

    public bool BindProperty(string propertyName, object value, bool destructureObjects, out LogEventProperty property)
    {
        property = null;
        return false;
    }
}
