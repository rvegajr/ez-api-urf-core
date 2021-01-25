using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EzApiCore.Api
{
    /// <summary></summary>
    public class CustomDocumentFilter 
    {
        public static ApiDescription fResolveConflictingActions(IEnumerable<ApiDescription> apiDescriptions)
        {
            return apiDescriptions.First();
        }
    }

    /// <summary></summary>
    public class MultipleOperationsWithSameVerbFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            /*
            if (operation.parameters != null)
            {
                operation.operationId += "By";
                foreach (var parm in operation.parameters)
                {
                    operation.operationId += string.Format("{0}", parm.name);
                }             
             */
            throw new NotImplementedException();
        }
    }

    internal static class ApiDescriptionConflictResolver
    {
        public static ApiDescription Resolve(IEnumerable<ApiDescription> descriptions, string httpMethod)
        {
            var parameters = descriptions
              .SelectMany(desc => desc.ParameterDescriptions)
              .GroupBy(x => x, (x, xs) => new { IsOptional = xs.Count() == 1, Parameter = x }, ApiParameterDescriptionEqualityComparer.Instance)
              .ToList();
            var description = descriptions.First();
            description.ParameterDescriptions.Clear();
            parameters.ForEach(x =>
            {
                if (x.Parameter.RouteInfo != null)
                    x.Parameter.RouteInfo.IsOptional = x.IsOptional;
                description.ParameterDescriptions.Add(x.Parameter);
            });
            return description;
        }
    }

    internal class ODataRequestBodyFilter : IRequestBodyFilter
    {
        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            throw new NotImplementedException();
        }
    }

    internal class ODataOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //throw new NotImplementedException();
        }
    }

    /// <summary></summary>
    internal class ODataRenderDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            //throw new NotImplementedException();
        }

        /*
        /// <summary></summary>
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {

            var thisAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes().ToList();

            var odatacontrollers = thisAssemblyTypes.Where(t => t.IsSubclassOf(typeof(ODataController))).ToList();
            var odataRoutes = GlobalConfiguration.Configuration.Routes.Where(a => a.GetType() == typeof(ODataRoute)).ToList();

            if (!odataRoutes.Any() || !odatacontrollers.Any()) return;
            var odatamethods = new[] { "Get", "Put", "Post", "Delete" };

            var route = odataRoutes.FirstOrDefault() as ODataRoute;

            foreach (var odataContoller in odatacontrollers)  // this is all of the OData controllers in your API
            {
                var methods = odataContoller.GetMethods().Where(a => odatamethods.Contains(a.Name)).ToList();
                if (!methods.Any())
                    continue; // next controller -- this one doesn't have any applicable methods

                foreach (var method in methods)  // this is all of the methods for a SINGLE controller (e.g. GET, POST, PUT, etc)
                {
                    var path = "/" + route.RoutePrefix + "/" + odataContoller.Name.Replace("Controller", "");

                    if (swaggerDoc.paths.ContainsKey(path))
                    {
                        Debug.WriteLine("Path " + path + " already exists");
                        Console.WriteLine("Path " + path + " already exists");
                        continue;
                    }

                    var odataPathItem = new PathItem();
                    var op = new Operation();

                    // This is assuming that all of the odata methods will be listed under a heading called OData in the swagger doc
                    op.tags = new List<string> { "OData" };
                    op.operationId = "OData_" + odataContoller.Name.Replace("Controller", "");

                    // This should probably be retrieved from XML code comments....
                    op.summary = "Used to access odata endpoint for " + odataContoller.Name.Replace("Controller", "");
                    var exampleText = @"Some common OData examples are:<br/>
<a href='{0}/api/{1}' target=""_blank"">{0}/api/{1}</a> - Will get all objects of this type<br/>
<a href='{0}/api/{1}(1)' target=""_blank"">{0}/api/{1}(1)</a> - Will get the object of id=1<br/>
<a href='{0}/api/{1}?$top=2' target=""_blank"">{0}/api/{1}?$top=2</a>  Will get the Top 2 records<br/>
<a href='{0}/api/{1}?$select=FieldA%2CFieldB' target=""_blank"">{0}/api/{1}?$select=FieldA%2CFieldB</a> - Will select FieldA and FieldB only (obviously these fields need to exist in the table)";
                    op.description = string.Format(exampleText, "http://" + swaggerDoc.host, odataContoller.Name.Replace("Controller", ""));

                    op.consumes = new List<string>();
                    op.produces = new List<string> { "application/json", "text/json" };
                    op.deprecated = false;

                    var response = new Response() { description = "OK" };
                    response.schema = new Schema { type = "array", items = schemaRegistry.GetOrRegister(method.ReturnType) };
                    op.responses = new Dictionary<string, Response> { { "200", response } };

                    var security = GetSecurityForOperation(odataContoller);
                    if (security != null)
                        op.security = new List<IDictionary<string, IEnumerable<string>>> { security };

                    odataPathItem.get = op;   // this needs to be a switch based on the method name
                    if (swaggerDoc.paths.ContainsKey(path))
                    {
                        Debug.WriteLine("Path " + path + " already exists");
                        Console.WriteLine("Path " + path + " already exists");
                    }
                    else
                    {
                        swaggerDoc.paths.Add(path, odataPathItem);
                    }
                }
            }
        }
        */

        private Dictionary<string, IEnumerable<string>> GetSecurityForOperation(MemberInfo odataContoller)
        {
            Dictionary<string, IEnumerable<string>> securityEntries = null;
            if (odataContoller.GetCustomAttribute(typeof(AuthorizeAttribute)) != null)
            {
                securityEntries = new Dictionary<string, IEnumerable<string>> { { "oauth2", new[] { "actioncenter" } } };
            }
            return securityEntries;
        }
    }

    internal sealed class ApiParameterDescriptionEqualityComparer : IEqualityComparer<ApiParameterDescription>
    {
        private static readonly Lazy<ApiParameterDescriptionEqualityComparer> _instance
            = new Lazy<ApiParameterDescriptionEqualityComparer>(() => new ApiParameterDescriptionEqualityComparer());
        public static ApiParameterDescriptionEqualityComparer Instance
            => _instance.Value;

        private ApiParameterDescriptionEqualityComparer() { }

        public int GetHashCode(ApiParameterDescription obj)
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + obj.ModelMetadata.GetHashCode();
                hash = hash * 23 + obj.Name.GetHashCode();
                hash = hash * 23 + obj.Source.GetHashCode();
                hash = hash * 23 + obj.Type.GetHashCode();
                return hash;
            }
        }

        public bool Equals(ApiParameterDescription x, ApiParameterDescription y)
        {
            if (!x.ModelMetadata.Equals(y.ModelMetadata)) return false;
            if (!x.Name.Equals(y.Name)) return false;
            if (!x.Source.Equals(y.Source)) return false;
            if (!x.Type.Equals(y.Type)) return false;
            return true;
        }
    }
}
