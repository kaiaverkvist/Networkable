namespace Networkable.Listenable;

public class NetworkableServer : IListenable
{
    private readonly IListenable _listenable;

    public NetworkableServer(IListenable listenable)
    {
        _listenable = listenable;
    }

    public void Start() => _listenable.Start();
    public void Stop() => _listenable.Stop();
}