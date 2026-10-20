using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp1;
class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public AppDbContext(DbContextOptions opt) : base(opt)
    {

    }
}
