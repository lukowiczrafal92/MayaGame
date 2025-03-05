
namespace BoardGameBackend.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class EventHandlerWithPriority<T>
    {
        public Action<T> Handler { get; }
        public int Priority { get; }

        public EventHandlerWithPriority(Action<T> handler, int priority)
        {
            Handler = handler;
            Priority = priority;
        }
    }

    internal class EventHandlerWithPriority
    {
        public Action Handler { get; }
        public int Priority { get; }

        public EventHandlerWithPriority(Action handler, int priority)
        {
            Handler = handler;
            Priority = priority;
        }
    }

    public class EventManager
    {
        private readonly Dictionary<string, List<object>> _eventHandlers = new Dictionary<string, List<object>>();

        public void Subscribe<T>(string eventName, Action<T> handler, int priority = 0)
        {
            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = new List<object>();
            }

            _eventHandlers[eventName].Add(new EventHandlerWithPriority<T>(handler, priority));

            _eventHandlers[eventName] = _eventHandlers[eventName]
                .Cast<EventHandlerWithPriority<T>>()
                .OrderByDescending(h => h.Priority)
                .Cast<object>()
                .ToList();
        }

        public void Subscribe(string eventName, Action handler, int priority = 0)
        {
            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = new List<object>();
            }

            _eventHandlers[eventName].Add(new EventHandlerWithPriority(handler, priority));

            _eventHandlers[eventName] = _eventHandlers[eventName]
                .Cast<EventHandlerWithPriority>()
                .OrderByDescending(h => h.Priority)
                .Cast<object>()
                .ToList();
        }

        public void Unsubscribe<T>(string eventName, Action<T> handler)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                var handlers = _eventHandlers[eventName]
                    .Cast<EventHandlerWithPriority<T>>()
                    .Where(h => h.Handler != handler)
                    .Cast<object>()
                    .ToList();

                _eventHandlers[eventName] = handlers;
            }
        }

        public void Unsubscribe(string eventName, Action handler)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                var handlers = _eventHandlers[eventName]
                    .Cast<EventHandlerWithPriority>()
                    .Where(h => h.Handler != handler)
                    .Cast<object>()
                    .ToList();

                _eventHandlers[eventName] = handlers;
            }
        }

        public void Broadcast<T>(string eventName, ref T eventData)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                foreach (var handlerObj in _eventHandlers[eventName])
                {
                    if (handlerObj is EventHandlerWithPriority<T> handlerWithPriority)
                    {
                        handlerWithPriority.Handler.Invoke(eventData);
                    }
                }
            }
        }


        public void Broadcast(string eventName)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                foreach (var handlerObj in _eventHandlers[eventName])
                {
                    if (handlerObj is EventHandlerWithPriority handlerWithPriority)
                    {
                        handlerWithPriority.Handler.Invoke();
                    }
                }
            }
        }
    }


}