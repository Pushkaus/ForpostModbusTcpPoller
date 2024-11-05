using ForpostModbusTcpPoller.Models;
using ForpostModbusTcpPoller.Database;
using Microsoft.EntityFrameworkCore;

namespace ForpostModbusTcpPoller.Services
{
    public class DeviceManagerService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public DeviceManagerService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<ForpostModbusDevice>> GetAllDevicesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Devices.ToListAsync();
        }

        public async Task<ForpostModbusDevice> GetDeviceByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Devices.FindAsync(id);
        }

        public async Task<ForpostModbusDevice> AddDeviceAsync(ForpostModbusDevice device)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var nextId = 1;
            while (await context.Devices.AnyAsync(d => d.Id == nextId))
            {
                nextId++;
            }
            device.Id = nextId;
            context.Devices.Add(device);
            await context.SaveChangesAsync();
            return device;
        }


        public async Task<bool> UpdateDeviceAsync(ForpostModbusDevice device)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.Update(device);

            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteDeviceAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var device = await context.Devices.FindAsync(id);
            if (device == null)
            {
                return false;
            }

            context.Devices.Remove(device);
            await context.SaveChangesAsync();
            return true;
        }
    }
}