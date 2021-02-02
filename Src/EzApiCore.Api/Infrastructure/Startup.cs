// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Orders.Api.Domain;
using Orders.Api.Repository;

namespace EzApiCore.Api
{
    /// <summary>
    ///     Configuration for the web app.
    /// </summary>
    public class Startup
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        ///     Gets the configuration.
        /// </summary>
        /// <value>
        ///     The configuration.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(x =>
            {
                x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                x.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Orders Public API",
                    Version = "v1",
                    Description = "An open-source, .NET 5, OData-enabled, Web API that implements best practices around test-driven development, application layering, inversion of control, RESTful design, IO performance (async/await), and nullability.",
                    Contact = new OpenApiContact
                    {
                        Name = "Richard Beauchamp",
                        Email = string.Empty,
                        Url = new Uri("https://github.com/rbeauchamp/orders-api"),
                    },
                });

                c.EnableAnnotations();

                // See https://stackoverflow.com/a/53886604
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Orders.Api.xml"));
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Orders.Api.Domain.xml"));
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Orders.Api.Model.xml"));
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Orders.Api.Repository.xml"));
            });

            // services.AddSwaggerGenNewtonsoftSupport();
            services.AddDbContext<IOrdersRepository, OrdersRepository>(optionsBuilder => optionsBuilder.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddScoped<IOrdersDomain, OrdersDomain>();
            services.AddScoped<SubmitOrderValidator>();
            services.AddScoped<UpdateOrderValidator>();

            services.AddOData();

            services.AddMvc(
                options =>
                {
                    options.EnableEndpointRouting = false;

                    // Required to generate swagger documentation for an API that has OData endpoints
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

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API v1");
                c.EnableDeepLinking();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseMvc(routeBuilder =>
            {
                routeBuilder.EnableDependencyInjection();
                routeBuilder.Expand().Select().OrderBy().Filter().Count().MaxTop(10);
            });
        }
    }
}