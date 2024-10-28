using ForpostModbusTcpPoller.Models;
using ForpostModbusTcpPoller.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForpostModbusTcpPoller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModbusDevicesController : ControllerBase
    {
        private readonly DeviceManagerService _deviceManager;

        public ModbusDevicesController(DeviceManagerService deviceManager)
        {
            _deviceManager = deviceManager;
        }

        /// <summary>
        /// Получает список всех Modbus устройств.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ForpostModbusDevice>>> GetDevices()
        {
            var devices = await _deviceManager.GetAllDevicesAsync();
            return Ok(devices);
        }

        /// <summary>
        /// Получает конкретное Modbus устройство по идентификатору.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ForpostModbusDevice>> GetDevice(int id)
        {
            var device = await _deviceManager.GetDeviceByIdAsync(id);
            if (device == null)
                return NotFound();

            return Ok(device);
        }

        /// <summary>
        /// Добавляет новое Modbus устройство.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ForpostModbusDevice>> AddDevice(ForpostModbusDevice device)
        {
            var addedDevice = await _deviceManager.AddDeviceAsync(device);
            return CreatedAtAction(nameof(GetDevice), new { id = addedDevice.Id }, addedDevice);
        }

        /// <summary>
        /// Обновляет существующее Modbus устройство.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(int id, ForpostModbusDevice device)
        {
            if (id != device.Id)
                return BadRequest();

            var updated = await _deviceManager.UpdateDeviceAsync(device);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Удаляет Modbus устройство по идентификатору.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var deleted = await _deviceManager.DeleteDeviceAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
