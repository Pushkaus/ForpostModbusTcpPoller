using System.Collections.Concurrent;
using ForpostModbusTcpPoller.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ForpostModbusTcpPoller.Services
{
    public class DeviceManagerService
    {
        private readonly ConcurrentDictionary<int, ForpostModbusDevice> _devices = new();
        private int _currentId = 0;

        /// <summary>
        /// Асинхронно получает список всех Modbus устройств.
        /// </summary>
        /// <returns>Список всех устройств.</returns>
        public Task<IEnumerable<ForpostModbusDevice>> GetAllDevicesAsync()
        {
            return Task.FromResult(_devices.Values.ToList() as IEnumerable<ForpostModbusDevice>);
        }

        /// <summary>
        /// Асинхронно получает Modbus устройство по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор устройства.</param>
        /// <returns>Устройство, если найдено; иначе null.</returns>
        public Task<ForpostModbusDevice> GetDeviceByIdAsync(int id)
        {
            _devices.TryGetValue(id, out var device);
            return Task.FromResult(device);
        }

        /// <summary>
        /// Асинхронно добавляет новое Modbus устройство.
        /// </summary>
        /// <param name="device">Модель нового устройства.</param>
        /// <returns>Добавленное устройство.</returns>
        public Task<ForpostModbusDevice> AddDeviceAsync(ForpostModbusDevice device)
        {
            var id = Interlocked.Increment(ref _currentId);
            device.Id = id;
            _devices[id] = device;
            return Task.FromResult(device);
        }

        /// <summary>
        /// Асинхронно обновляет существующее Modbus устройство.
        /// </summary>
        /// <param name="device">Обновленная модель устройства.</param>
        /// <returns>true, если устройство обновлено; иначе false.</returns>
        public Task<bool> UpdateDeviceAsync(ForpostModbusDevice device)
        {
            if (!_devices.ContainsKey(device.Id))
                return Task.FromResult(false);

            _devices[device.Id] = device;
            return Task.FromResult(true);
        }

        /// <summary>
        /// Асинхронно удаляет Modbus устройство по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор устройства.</param>
        /// <returns>true, если устройство удалено; иначе false.</returns>
        public Task<bool> DeleteDeviceAsync(int id)
        {
            return Task.FromResult(_devices.TryRemove(id, out _));
        }

        // Метод для получения всех устройств (для тестирования или других целей)
        public IEnumerable<ForpostModbusDevice> GetDevicesForSaving()
        {
            return _devices.Values.ToList();
        }
    }
}
