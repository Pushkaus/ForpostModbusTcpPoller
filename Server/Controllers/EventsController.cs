using ForpostModbusTcpPoller.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ForpostModbusTcpPoller.Models;

namespace ForpostModbusTcpPoller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly EventService _eventService;

    public EventsController(EventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEvents([FromQuery] int skip = 0, [FromQuery] int limit = 10)
    {
        var events = await _eventService.GetAllEventsAsync(skip, limit);
        return Ok(events);
    }
    [HttpGet("Unconfirmed")]
    public async Task<IActionResult> GetAllUnconfirmedEvents([FromQuery] int skip = 0, [FromQuery] int limit = 10)
    {
        var events = await _eventService.GetUnconfirmedEventsAsync(skip, limit);
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(Guid id)
    {
        var eventItem = await _eventService.GetEventByIdAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }

        return Ok(eventItem);
    }
    [HttpPost]
    public async Task<IActionResult> AddEvent(Event newEvent)
    {
        if (newEvent == null)
        {
            return BadRequest("Event data is required.");
        }

        await _eventService.AddEventAsync(newEvent);
        return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        await _eventService.DeleteEventAsync(id);
        return NoContent();
    }
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmEvent(Guid id)
    {
        try
        {
            await _eventService.SetConfirmed(id);
            return NoContent(); 
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [HttpPost("{id}/fixed")]
    public async Task<IActionResult> FixedEvent(Guid id)
    {
        try
        {
            await _eventService.SetFixed(id);
            return NoContent(); 
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}