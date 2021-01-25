using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EzApiCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using URF.Core.EF;
using URF.Core.Abstractions.Trackable;
using URF.Core.EF.Trackable;
using URF.Core.Abstractions;
using EzApiCore.Service;
using EzApiCore.Repository;

namespace EzApiCore.Api
{
    internal class UnityBuilder
    {
        public UnityBuilder(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void UnityConfiguration(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString(nameof(EzApiCoreContext));
            services.AddDbContext<EzApiCoreContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<DbContext, EzApiCoreContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITrackableRepository<Products>, TrackableRepository<Products>>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ITrackableRepository<OrderDetails>, TrackableRepository<OrderDetails>>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();

            // Example: extending IRepository<TEntity>, scope: application-wide and IService<TEntity>, scope: ICustomerService
            services.AddScoped<IRepositoryX<Customers>, RepositoryX<Customers>>();
            services.AddScoped<ICustomerService, CustomerService>();

            services.AddScoped<ITrackableRepository<Categories>, TrackableRepository<Categories>>();
            services.AddScoped<ICategoriesService, CategoriesService>();
        }
    }
}
