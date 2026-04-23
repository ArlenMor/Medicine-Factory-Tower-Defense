using System;
using System.Collections.Generic;

namespace _Project.Code.Scripts.ServiceLocator
{
    public static class S
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException(
                $"[ServiceLocator] Service of type '{typeof(T).Name}' is not registered. " +
                "Make sure it is registered in Bootstrap before use.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var raw))
            {
                service = (T)raw;
                return true;
            }

            service = null;
            return false;
        }
        
        public static void Reset()
        {
            _services.Clear();
        }
    }
}
