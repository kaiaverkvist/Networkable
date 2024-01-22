using System.Reflection;

namespace Networkable.EfficientInvoker.Extensions;

public static class MethodBaseExtensions
{
    public static IReadOnlyList<Type> GetParameterTypes(this MethodBase method)
    {
        return TypeExtensions.ParameterMap.GetOrAdd(method, c => c.GetParameters().Select(p => p.ParameterType).ToArray());
    }
}