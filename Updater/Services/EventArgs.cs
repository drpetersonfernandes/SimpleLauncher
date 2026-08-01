namespace Updater.Services;

/// <summary>
/// Provides a strongly typed <see cref="EventArgs"/> subclass wrapping a single value.
/// </summary>
/// <typeparam name="T">The type of the wrapped value.</typeparam>
public sealed class EventArgs<T> : EventArgs
{
    public EventArgs(T value)
    {
        Value = value;
    }

    /// <summary>Gets the wrapped value.</summary>
    public T Value { get; }
}
