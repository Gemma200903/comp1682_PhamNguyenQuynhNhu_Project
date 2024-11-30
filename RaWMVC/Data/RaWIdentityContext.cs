using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;

namespace RaWMVC.Data
{
    public class RaWIdentityContext : IdentityDbContext<RaWMVCUser, RaWMVCRole, string, IdentityUserClaim<string>, IdentityUserRole<string>,
        IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public RaWIdentityContext(DbContextOptions<RaWIdentityContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Server=QUYNHNHU;Database=RaWIdentityContext;User Id=itsjuneka; password=Quynhnhu@14; TrustServerCertificate=True; Trusted_Connection=False; MultipleActiveResultSets=true;";
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<RaWMVCRole> Role { get; set; }
    }
}