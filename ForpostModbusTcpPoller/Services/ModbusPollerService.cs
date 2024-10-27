using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using ForpostModbusTcpPoller.Hubs;
using ForpostModbusTcpPoller.Models;
using Microsoft.AspNetCore.SignalR;
using Modbus.Device;

namespace ForpostModbusTcpPoller.Services
{
    public class ModbusPollerService
    {
        private readonly DeviceManagerService _deviceManager;
        private readonly IHubContext<ModbusHub> _hubContext;

        public ModbusPollerService(DeviceManagerService deviceManager, IHubContext<ModbusHub> hubContext)
        {
            _deviceManager = deviceManager;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Асинхронно опрашивает все устройства.
        /// </summary>
        public async Task PollDevicesAsync()
        {
            var devices = await _deviceManager.GetAllDevicesAsync();
            
            var pollTasks = new List<Task>();

            foreach (var device in devices)
            {
                pollTasks.Add(PollDeviceAsync(device)); // Добавляем каждый опрос в список задач
            }

            await Task.WhenAll(pollTasks); 
        }

        /// <summary>
        /// Асинхронно опрашивает конкретное Modbus устройство.
        /// </summary>
        /// <param name="device">Устройство для опроса.</param>
        private async Task PollDeviceAsync(ForpostModbusDevice device)
        {
            try
            {
                using var client = new TcpClient();
                Console.WriteLine($"Attempting to connect to device {device.IpAddress}:{device.Port}");
                await client.ConnectAsync(device.IpAddress, device.Port);
                Console.WriteLine("Successfully connected to the device.");

                using var master = ModbusIpMaster.CreateIp(client);
                master.Transport.ReadTimeout = 5000; // Увеличенный таймаут чтения
                master.Transport.WriteTimeout = 5000; // Увеличенный таймаут записи
                
                ushort startAddress = device.RegisterAddress; // Начальный адрес
                ushort numberOfPoints = 1; // Количество регистров для чтения

                ushort[] registers = await master.ReadInputRegistersAsync(device.UnitId, startAddress, numberOfPoints);

                var data = new
                {
                    DeviceId = device.Id,
                    device.IpAddress,
                    device.Port,
                    device.RegisterAddress,
                    device.RegisterName,
                    Value = registers[0], // Считываем значение из первого регистра
                    Timestamp = DateTime.UtcNow // Текущее время
                };

                await _hubContext.Clients.All.SendAsync("ReceiveData", data);
                Console.WriteLine($"Received register value {registers[0]} from device {device.IpAddress}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket Error while connecting to device {device.IpAddress}:{device.Port}. Check connection and network.");
                Console.WriteLine($"Details: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid Operation with device {device.IpAddress}:{device.Port}. Check if the socket is connected.");
                Console.WriteLine($"Details: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO Exception while reading from device {device.IpAddress}:{device.Port}. Check if the device is online and reachable.");
                Console.WriteLine($"Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while polling device {device.IpAddress}:{device.Port}");
                Console.WriteLine($"Details: {ex.Message}");
            }
        }
    }
}
