namespace WebApi.Helpers;

using Microsoft.EntityFrameworkCore;
using WebApi.Entities;

public class DataContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DataContext() { }

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // in memory database used for simplicity, change to a real db for production applications
        //options.UseInMemoryDatabase("TestDb");

        options.UseSqlServer(Configuration.GetConnectionString("UserDB"));
    }

    public virtual DbSet<User> Users { get; set; }
}