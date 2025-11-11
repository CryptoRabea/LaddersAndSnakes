using System;
using System.Collections.Generic;


namespace LAS.Core
{
    public static class ServiceLocator
    {
        static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        public static void Register<T>(T service) where T : class
        {
            var t = typeof(T);
            if (services.ContainsKey(t)) services[t] = service; else services.Add(t, service);
        }
        public static T Get<T>() where T : class
        {
            services.TryGetValue(typeof(T), out var svc); return svc as T;
        }
        public static void Clear() => services.Clear();
    }
}