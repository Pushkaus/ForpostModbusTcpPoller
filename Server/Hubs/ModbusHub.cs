﻿using Microsoft.AspNetCore.SignalR;

namespace ForpostModbusTcpPoller.Hubs
{
    public class ModbusHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}