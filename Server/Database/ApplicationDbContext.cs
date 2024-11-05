using ForpostModbusTcpPoller.Models;
using Microsoft.EntityFrameworkCore;


namespace ForpostModbusTcpPoller.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ForpostModbusDevice> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ForpostModbusDevice>().HasIndex(x => x.IpAddress).IsUnique();
    }
}