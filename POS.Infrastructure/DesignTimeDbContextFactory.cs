using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using POS.Infrastructure.Data;

namespace POS.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            optionsBuilder.UseSqlServer(
                "Server=localhost\\SQLEXPRESS;Database=POSSalsamentaria;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}