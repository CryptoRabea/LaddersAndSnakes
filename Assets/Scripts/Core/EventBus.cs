using System;
using System.Collections.Generic;


namespace LAS.Core
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> callback);
        void Unsubscribe<T>(Action<T> callback);
        void Publish<T>(T evt);
    }


    public class EventBus : IEventBus
    {
        readonly Dictionary<Type, Delegate> table = new Dictionary<Type, Delegate>();
        public void Subscribe<T>(Action<T> callback)
        {
            var t = typeof(T);
            if (table.TryGetValue(t, out var d)) table[t] = Delegate.Combine(d, callback);
            else table[t] = callback;
        }
        public void Unsubscribe<T>(Action<T> callback)
        {
            var t = typeof(T);
            if (table.TryGetValue(t, out var d))
            {
                var nd = Delegate.Remove(d, callback);
                if (nd == null) table.Remove(t); else table[t] = nd;
            }
        }
        public void Publish<T>(T evt)
        {
            var t = typeof(T);
            if (table.TryGetValue(t, out var d)) (d as Action<T>)?.Invoke(evt);
        }
    }
}