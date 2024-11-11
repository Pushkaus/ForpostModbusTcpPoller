namespace ForpostModbusTcpPoller.Models;

public class Event
{
    public int Id { get; set; }
    public string IpAdress { get; set; }
    public EventStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
