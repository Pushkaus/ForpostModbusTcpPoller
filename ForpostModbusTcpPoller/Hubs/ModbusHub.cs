// Backend/Hubs/ModbusHub.cs

using Microsoft.AspNetCore.SignalR;

namespace ForpostModbusTcpPoller.Hubs
{
    public class ModbusHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            // Дополнительная логика при подключении клиента
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            // Дополнительная логика при отключении клиента
        }

        // Можно добавить методы для двунаправленной связи, если необходимо
    }
}