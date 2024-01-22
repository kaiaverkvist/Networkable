using System.Collections.Concurrent;
using System.Reflection;

namespace Networkable.EfficientInvoker.Extensions;

public static class TypeExtensions
{
    internal static readonly ConcurrentDictionary<MethodBase, IReadOnlyList<Type>> ParameterMap = new();
        
    public static EfficientInvoker GetMethodInvoker(this Type type, string methodName)
    {
        return EfficientInvoker.ForMethod(type, methodName);
    }

    public static EfficientInvoker GetPropertyInvoker(this Type type, string propertyName)
    {
        return EfficientInvoker.ForProperty(type, propertyName);
    }
}