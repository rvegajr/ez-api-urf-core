using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
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
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //services.AddControllers(mvcOptions => mvcOptions.EnableEndpointRouting = false);
            services.AddOData();
            UnityConfiguration(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            /* Migrating 2.X to 3.1 - https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-3.1&tabs=visual-studio
             * OData Temporary Fix for 3.1 - https://devblogs.microsoft.com/odata/experimenting-with-odata-in-asp-net-core-3-1/
             * Odata does not support the End points yet
            */
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.Select().Expand().Filter().OrderBy().MaxTop(1000).Count();
                endpoints.MapODataRoute("ODataRoute", "odata", GetEdmModel());
            });
            /*
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.Select().Expand().Filter().OrderBy().MaxTop(1000).Count();
                routeBuilder.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            */
        }

        private void UnityConfiguration(IServiceCollection services) {
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

        private IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            var categoriesEntitySetConfiguration = odataBuilder.EntitySet<Categories>(nameof(Categories));
            categoriesEntitySetConfiguration.EntityType.HasKey(x => x.CategoryId);

            var customersEntitySetConfiguration = odataBuilder.EntitySet<Customers>(nameof(Customers));
            customersEntitySetConfiguration.EntityType.HasKey(x => x.CustomerId);
            customersEntitySetConfiguration.EntityType.Ignore(x => x.CustomerCustomerDemo);
            customersEntitySetConfiguration.EntityType.Ignore(x => x.Orders);

            var productsEntitySetConfiguration = odataBuilder.EntitySet<Products>(nameof(Products));
            productsEntitySetConfiguration.EntityType.HasKey(x => x.ProductId);
            productsEntitySetConfiguration.EntityType.Ignore(x => x.Category);
            productsEntitySetConfiguration.EntityType.Ignore(x => x.Supplier);

            var orderDetailsEntitySetConfiguration = odataBuilder.EntitySet<OrderDetails>(nameof(OrderDetails));
            orderDetailsEntitySetConfiguration.EntityType.HasKey(x => x.OrderId);
            orderDetailsEntitySetConfiguration.EntityType.HasKey(x => x.ProductId);
            orderDetailsEntitySetConfiguration.EntityType.Ignore(x => x.Order);

            return odataBuilder.GetEdmModel();
        }
    }
}
