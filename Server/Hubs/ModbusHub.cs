using Microsoft.AspNetCore.SignalR;

namespace ForpostModbusTcpPoller.Hubs
{
    public class ModbusHub : Hub
    {
        private static readonly List<string> ConnectedClients = new List<string>();
        public override async Task OnConnectedAsync()
        {
            ConnectedClients.Add(Context.ConnectionId);
            Console.WriteLine($"Клиент подключен: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
        public Task<List<string>> GetConnectedClients()
        {
            return Task.FromResult(new List<string>(ConnectedClients));
        }
    }
}