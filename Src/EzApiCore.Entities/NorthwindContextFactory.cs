using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EzApiCore.Data.Models;

namespace EzApiCore.Data
{
    public class EzApiCoreContextFactory : IDesignTimeDbContextFactory<EzApiCoreContext>
    {
        public EzApiCoreContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EzApiCoreContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=Northwind;Trusted_Connection=True;");
            return new EzApiCoreContext(optionsBuilder.Options);
        }
    }
}