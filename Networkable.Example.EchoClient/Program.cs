using Networkable.Builtin;
using Networkable.Serialization;
using Networkable.Transports;

Console.WriteLine("Hello, World!");

var client = new LiteNetworkableServer(new JsonNetworkSerializer(), "echo_server");
client.Router.Register<ClientConnected>((client, connected) =>
{
    Console.WriteLine($"{client}: Connected");
});
client.Router.Register<ClientDisconnected>((client, disconnected) =>
{
    Console.WriteLine($"{client}: Disconnected: {disconnected.Reason}");
});

client.Start();
client.Connect("localhost", 3022);

while (!Console.KeyAvailable)
{
    client.Poll();
}