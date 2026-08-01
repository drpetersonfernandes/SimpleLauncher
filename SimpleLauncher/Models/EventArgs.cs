namespace SimpleLauncher.Models;

/// <summary>
/// Provides a strongly typed <see cref="EventArgs"/> subclass wrapping a single value.
/// </summary>
/// <typeparam name="T">The type of the wrapped value.</typeparam>
public sealed class EventArgs<T> : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventArgs{T}"/> class with the specified value.
    /// </summary>
    /// <param name="value">The value to wrap in the event arguments.</param>
    public EventArgs(T value)
    {
        Value = value;
    }

    /// <summary>Gets the wrapped value.</summary>
    public T Value { get; }
}
