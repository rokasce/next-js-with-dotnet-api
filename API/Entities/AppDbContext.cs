using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Entities;

public class AppDbContext : IdentityDbContext<ApiUser>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
}