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
        var pagedResult = await _eventService.GetAllEventsAsync(skip, limit);
        return Ok(new
        {
            TotalCount = pagedResult.TotalCount,
            Items = pagedResult.Items
        });
    }

    [HttpGet("Unconfirmed")]
    public async Task<IActionResult> GetAllUnconfirmedEvents([FromQuery] int skip = 0, [FromQuery] int limit = 10)
    {
        var pagedResult = await _eventService.GetUnconfirmedEventsAsync(skip, limit);
        return Ok(new
        {
            TotalCount = pagedResult.TotalCount,
            Items = pagedResult.Items
        });
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
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        await _eventService.DeleteEventAsync(id);
        return NoContent();
    }

}