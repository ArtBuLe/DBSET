using ConsoleApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp1;
class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<CustomGroup> Groups => Set<CustomGroup>();

    public AppDbContext(DbContextOptions opt) : base(opt)
    {
    }
}
