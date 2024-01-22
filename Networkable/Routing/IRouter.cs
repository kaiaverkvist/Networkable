namespace Networkable.Routing;

public interface IRouter
{
    bool IsValidClassName(string className);
    Type? PayloadTypeLookup(string className);
}