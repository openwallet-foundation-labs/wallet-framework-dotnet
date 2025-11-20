namespace WalletFramework.Core.Events;

/// <summary>
/// Event Aggregator pattern implementation for decoupled event handling.
/// Allows components to communicate through events without direct dependencies.
/// </summary>
/// <remarks>
/// Modified from https://github.com/shiftkey/Reactive.EventAggregator
/// </remarks>
public interface IEventAggregator
{
    /// <summary>
    /// Gets an observable stream for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to observe</typeparam>
    /// <returns>An IObservable that emits events of type TEvent when they are published</returns>
    IObservable<TEvent> GetEventByType<TEvent>();

    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    /// <typeparam name="TEvent">The type of event being published</typeparam>
    /// <param name="eventToPublish">The event instance to publish to subscribers</param>
    void Publish<TEvent>(TEvent eventToPublish);
}
