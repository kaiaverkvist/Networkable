namespace Networkable.Builtin;

/// <summary>
/// Built in message that gets dispatched upon a client disconnecting.
/// Includes the DisconnectReason enum.
/// </summary>
public class ClientDisconnected
{
    public readonly string? Reason;

    public ClientDisconnected(string? reason = null)
    {
        Reason = reason;
    }
}