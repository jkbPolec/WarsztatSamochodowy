using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Worker> Workers { get; set; }
    public DbSet<PartType> PartTypes { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ServiceTask> ServiceTasks { get; set; }
    public DbSet<ServiceOrder> ServiceOrders { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<UsedPart> UsedParts { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Comment> Comments { get; set; }
    
}