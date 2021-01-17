using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using EzApiCore.Data.Models;
using URF.Core.Abstractions;
using URF.Core.EF;
using URF.Core.Abstractions.Trackable;
using URF.Core.EF.Trackable;
using EzApiCore.Service;
using EzApiCore.Repository;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Reflection;

namespace EzApiCore.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private void UnityConfiguration(IServiceCollection services)
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

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers().AddNewtonsoftJson();
            services.AddOData(opt => opt.AddModel("odata", GetEdmModel()).Count().Filter().OrderBy().Expand().Select().SetMaxTop(null));
            this.UnityConfiguration(services);
            services.AddSwaggerGen(c =>
            {
                c.DocumentFilter<CustomSwaggerDocumentAttribute>();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EzApiCore.Api", Version = "v1" });
            });
            services.AddMvcCore(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<OutputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }

                foreach (var inputFormatter in options.InputFormatters.OfType<InputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EzApiCore.Api v1"));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class CustomSwaggerDocumentAttribute : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var thisAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes().ToList();

            var odatacontrollers = thisAssemblyTypes.Where(t => t.IsSubclassOf(typeof(ODataController))).ToList();
        }
    }
}
