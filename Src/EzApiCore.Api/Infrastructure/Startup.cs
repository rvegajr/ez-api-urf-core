using EzApiCore.Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EzApiCore.Api
{
    public class ApiExplorerIgnores : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (action.Controller.ControllerName.Contains("GetMetadata"))
                action.ApiExplorer.IsVisible = false;
        }
    }
    public class Startup
    {
        private IEdmModel model;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            model = GetEdmModel();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            (new UnityBuilder(Configuration)).UnityConfiguration(services);

            services.AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(100)
                    .AddModel("odata", model)
                    );

            services.AddMvcCore(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                options.Conventions.Add(new ApiExplorerIgnores());
            }).AddApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EzApiCore.Api", Version = "v1" });
                c.ResolveConflictingActions(CustomDocumentFilter.fResolveConflictingActions);
                c.SwaggerGeneratorOptions.IgnoreHttpAttributeMissing = true;
                c.SwaggerGeneratorOptions.DocumentFilters.Add(new ODataRenderDocumentFilter());
                c.SwaggerGeneratorOptions.OperationFilters.Add(new ODataOperationFilter());
                c.SwaggerGeneratorOptions.RequestBodyFilters.Add(new ODataRequestBodyFilter());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EzApiCore.Api v1"));

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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
