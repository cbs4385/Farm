using System;
using System.Collections.Generic;

namespace Farm.Core
{
    // Typed publish/subscribe. Event payloads should be readonly structs to avoid allocations.
    public sealed class EventBus
    {
        readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _handlers.TryGetValue(typeof(T), out var existing);
            _handlers[typeof(T)] = Delegate.Combine(existing, handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null || !_handlers.TryGetValue(typeof(T), out var existing)) return;
            var remaining = Delegate.Remove(existing, handler);
            if (remaining == null) _handlers.Remove(typeof(T));
            else _handlers[typeof(T)] = remaining;
        }

        public void Publish<T>(T evt)
        {
            if (!_handlers.TryGetValue(typeof(T), out var del)) return;
            // Delegates are immutable, so handlers may unsubscribe while being invoked.
            foreach (var d in del.GetInvocationList())
            {
                try { ((Action<T>)d)(evt); }
                catch (Exception e) { Log.Error($"EventBus handler for {typeof(T).Name} threw: {e}"); }
            }
        }

        public void Clear() => _handlers.Clear();
    }
}
