using ForpostModbusTcpPoller.Database;
using Microsoft.EntityFrameworkCore;
using ForpostModbusTcpPoller.Models;

namespace ForpostModbusTcpPoller.Services;

public sealed class EventService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public EventService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(int skip = 0, int limit = 10)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Events
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetUnconfirmedEventsAsync(int skip = 0, int limit = 10)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Events
            .Where(e => e.Status == EventStatus.NotConfirmed)
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Events.FindAsync(id);
    }
    public async Task SetConfirmed(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var eventItem = await context.Events.FindAsync(id);
        if (eventItem == null)
        {
            throw new InvalidOperationException($"Event with ID {id} not found.");
        }
        eventItem.Status = EventStatus.Confirmed;
        await context.SaveChangesAsync();
    }
    public async Task SetFixed(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var eventItem = await context.Events.FindAsync(id);
        if (eventItem == null)
        {
            throw new InvalidOperationException($"Event with ID {id} not found.");
        }
        eventItem.Status = EventStatus.Fixed;
        await context.SaveChangesAsync();
    }


    public async Task AddEventAsync(Event newEvent)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        newEvent.CreatedAt = DateTime.Now;
        await context.Events.AddAsync(newEvent);
        await context.SaveChangesAsync();
    }

    public async Task DeleteEventAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var eventToDelete = await context.Events.FindAsync(id);
        if (eventToDelete != null)
        {
            context.Events.Remove(eventToDelete);
            await context.SaveChangesAsync();
        }
    }

    public async Task CheckEvent(ForpostModbusDevice device, bool currentIsWarning, PreviousData previousData)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        if (!currentIsWarning && previousData.IsWarning)
        {
            var newEvent = new Event
            {
                IpAdress = device.IpAddress,
                CreatedAt = DateTime.UtcNow,
                Status = previousData.IsConfirmed ? EventStatus.Fixed : EventStatus.NotConfirmed
            };
            context.Events.Add(newEvent);
            await context.SaveChangesAsync();
        }
        else if (currentIsWarning && !previousData.IsWarning)
        {
            if (!device.IsConfirmed)
            {
                var newEvent = new Event
                {
                    IpAdress = device.IpAddress,
                    CreatedAt = DateTime.UtcNow,
                    Status = EventStatus.NotConfirmed
                };
                context.Events.Add(newEvent);
                await context.SaveChangesAsync();
            }
        }
        else if (currentIsWarning && previousData.IsWarning && device.IsConfirmed && !previousData.IsConfirmed) 
        {
            var newEvent = new Event
            {
                IpAdress = device.IpAddress,
                CreatedAt = DateTime.UtcNow,
                Status = EventStatus.Confirmed
            };
            context.Events.Add(newEvent);
            await context.SaveChangesAsync();
        }
    }
}
