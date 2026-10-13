using Microsoft.EntityFrameworkCore;

namespace TerapiaDeApoio.Infrastructure.Data;

public class TerapiaDeApoioDbContext : DbContext
{
    public TerapiaDeApoioDbContext(
        DbContextOptions<TerapiaDeApoioDbContext> options)
        : base(options)
    {
    }

}