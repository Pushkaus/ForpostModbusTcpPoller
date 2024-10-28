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
        private readonly ILogger<ModbusPollingHostedService> _logger;

        public ModbusPollerService(DeviceManagerService deviceManager, IHubContext<ModbusHub> hubContext,
            ILogger<ModbusPollingHostedService> logger)
        {
            _deviceManager = deviceManager;
            _hubContext = hubContext;
            _logger = logger;
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
                await client.ConnectAsync(device.IpAddress, device.Port);
                using var master = ModbusIpMaster.CreateIp(client);
                master.Transport.ReadTimeout = 5000; // Увеличенный таймаут чтения
                master.Transport.WriteTimeout = 5000; // Увеличенный таймаут записи

                ushort startAddress = device.RegisterAddress; // Начальный адрес
                ushort numberOfPoints = 1; // Количество регистров для чтения

                ushort[] registers = await master.ReadInputRegistersAsync(device.UnitId, startAddress, numberOfPoints);
                var isWarning = registers[0] != 0;
                var data = new
                {
                    DeviceId = device.Id,
                    device.IpAddress,
                    device.Port,
                    device.RegisterAddress,
                    device.RegisterName,
                    Value = registers[0],
                    Timestamp = DateTimeOffset.UtcNow,
                    IsWarning = isWarning
                };
                await _hubContext.Clients.All.SendAsync("ReceiveData", data);
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex,
                    $"Ошибка сокета при подключении к устройству {device.IpAddress}:{device.Port}." +
                    $" Проверьте подключение и сеть.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex,
                    $"Некорректная операция с устройством {device.IpAddress}:{device.Port}." +
                    $" Проверьте подключение сокета.");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex,
                    $"Ошибка ввода-вывода при чтении с устройства {device.IpAddress}:{device.Port}." +
                    $" Проверьте, что устройство онлайн и доступно.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при опросе устройства {device.IpAddress}:{device.Port}");
            }
        }
    }
}