// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Orders.Api
{
    /// <summary>
    /// Enables OData query options to be passed via the Swagger UI to an OData endpoint
    /// that returns a single result.
    /// </summary>
    /// <see href="https://docs.microsoft.com/en-us/odata/concepts/queryoptions-overview"/>
    public class SingleResultODataQueryParams
    {
        /// <summary>
        /// Specify related resources to be included in line with retrieved resources.
        /// </summary>
        [FromQuery]
        [JsonProperty("$expand")]
        [ModelBinder(Name = "$expand")]
        public string? Expand { get; set; }

        /// <summary>
        /// Request a specific set of properties for each resource.
        /// </summary>
        [FromQuery]
        [JsonProperty("$select")]
        [ModelBinder(Name = "$select")]
        public string? Select { get; set; }
    }
}