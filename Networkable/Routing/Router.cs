using System.Security;
using Networkable.EfficientInvoker.Extensions;

namespace Networkable.Routing;

/// <summary>
/// Router translates network messages into calls to registered handlers. 
/// </summary>
public class Router<U> : IRouter
{
    private Dictionary<string, Type> _typeLookup = new();
    private Dictionary<Type, HashSet<Delegate>> _handlers = new();
    public uint HandlerCount => (uint)_handlers.Count;
    
    public Router()
    {
    }

    public void Register<T>(Action<U, T> handler)
    {
        var t = typeof(T);

        if (!_handlers.ContainsKey(t))
            _handlers[t] = new HashSet<Delegate>();
        
        if(!_typeLookup.ContainsKey(t.ToString()))
            _typeLookup.Add(t.ToString(), t);
        
        _handlers[t].Add(handler);
    }

    public int Trigger(U client, object? message)
    {
        if (client == null)
            throw new Exception("Client was null in router trigger call");
        
        if (message == null || !_typeLookup.ContainsValue(message.GetType()))
            throw new SecurityException($"Invalid trigger type. Register first. Classname: {message.GetType()}");
        
        int callCount = 0;

        var t = message.GetType();
        foreach (var handlerPair in _handlers.Where(h => h.Key == t))
        {
            var actions = handlerPair.Value;
            foreach (var @delegate in actions)
            {
                var invoker = @delegate.GetInvoker();
                invoker.Invoke(@delegate, client, message);
                
                callCount++;
            }
        }
        
        return callCount;
    }

    public int ManualTrigger<T>(U client, T message)
    {
        if (client == null)
            throw new Exception("Client was null in manual router trigger call");

        int callCount = 0;
        
        foreach (var handlerPair in _handlers.Where(h => h.Key == typeof(T)))
        {
            if(message == null)
                continue;

            var actions = handlerPair.Value;
            foreach (var @delegate in actions)
            {
                var invoker = @delegate.GetInvoker();
                invoker.Invoke(@delegate, client, message);
                callCount++;
            }
        }
        
        return callCount;
    }
    
    public bool IsValidClassName(string className)
    {
        return _typeLookup.ContainsKey(className);
    }
    
    public Dictionary<Type, HashSet<Delegate>> GetHandlerDictionary()
    {
        return _handlers;
    }

    public Type? PayloadTypeLookup(string className)
    {
        var messageType = Extensions.TypeExtensions.GetTypeFromAssemblies(className);
        return _handlers.ContainsKey(messageType) ? _handlers.FirstOrDefault(e => e.Key == messageType).Key : null;
    }
}