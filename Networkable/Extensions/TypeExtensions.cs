namespace Networkable.Extensions;

public static class TypeExtensions
{
    /// <summary>
    /// Works like a regular Type.GetType(string) call, except it also
    /// performs a loop across GetAssemblies() if it isn't found.
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static Type? GetTypeFromAssemblies(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type != null) return type;
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(typeName);
            if (type != null)
                return type;
        }
        return null;
    }
}