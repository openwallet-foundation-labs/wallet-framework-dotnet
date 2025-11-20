using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace WalletFramework.Core.Events;

/// <inheritdoc />
//Modified from https://github.com/shiftkey/Reactive.EventAggregator
public class EventAggregator : IEventAggregator
{
    private readonly Subject<object> _subject = new();

    /// <inheritdoc />
    public IObservable<TEvent> GetEventByType<TEvent>() => _subject.OfType<TEvent>().AsObservable();

    /// <inheritdoc />
    public void Publish<TEvent>(TEvent eventToPublish) => _subject.OnNext(eventToPublish!);
}
