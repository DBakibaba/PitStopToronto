using Microsoft.EntityFrameworkCore;
using PitStop.API.Models;

namespace PitStop.API.Data;

public class PitStopDbContext(DbContextOptions<PitStopDbContext> options)
    : DbContext(options)
{
    public DbSet<Washroom> Washrooms => Set<Washroom>();
}