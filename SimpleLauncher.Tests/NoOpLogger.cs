#nullable disable
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace SimpleLauncher.Tests;

internal sealed class NoOpLogger : ILogger
{
    public void Write(LogEvent logEvent) { }

    public ILogger ForContext(ILogEventEnricher enricher) => this;
    public ILogger ForContext(IEnumerable<ILogEventEnricher> enrichers) => this;
    public ILogger ForContext(string propertyName, object value, bool destructureObjects = false) => this;

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
