﻿using ForpostModbusTcpPoller.Database;
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

    public async Task<PagedResult<Event>> GetAllEventsAsync(int skip = 0, int limit = 10)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var total = await context.Events.CountAsync();

        var items = await context.Events
            .Skip(skip)
            .Take(limit)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return new PagedResult<Event>
        {
            Items = items,
            TotalCount = total
        };
    }

    public async Task<PagedResult<Event>> GetUnconfirmedEventsAsync(int skip = 0, int limit = 10)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Events.Where(e => e.Status == EventStatus.NotConfirmed);
        var total = await query.CountAsync();

        var items = await query
            .Skip(skip)
            .Take(limit)
            .ToListAsync();

        return new PagedResult<Event>
        {
            Items = items,
            TotalCount = total
        };
    }

    public async Task<Event?> GetEventByIdAsync(int id)
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

    public async Task DeleteEventAsync(int id)
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
                CreatedAt = DateTime.Now,
                Status = EventStatus.Fixed
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
                    CreatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now,
                Status = EventStatus.Confirmed
            };
            context.Events.Add(newEvent);
            await context.SaveChangesAsync();
        }
    }
}