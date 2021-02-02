// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Orders.Api
{
    /// <summary>
    /// Enables OData query options to be passed via the Swagger UI to an OData endpoint.
    /// </summary>
    /// <see href="https://docs.microsoft.com/en-us/odata/concepts/queryoptions-overview"/>
    public class ODataQueryParams
    {
        /// <summary>
        /// Filter against a collection of resources.
        /// </summary>
        [FromQuery]
        [JsonProperty("$filter")]
        [ModelBinder(Name = "$filter")]
        public string? Filter { get; set; }

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

        /// <summary>
        /// Request resources in a particular order.
        /// </summary>
        [FromQuery]
        [JsonProperty("$orderby")]
        [ModelBinder(Name = "$orderby")]
        public string? OrderBy { get; set; }

        /// <summary>
        /// The number of items in the queried collection to be included in the result.
        /// </summary>
        [FromQuery]
        [JsonProperty("$top")]
        [ModelBinder(Name = "$top")]
        public int? Top { get; set; }

        /// <summary>
        /// The number of items in the queried collection that are to be skipped and not included in the result.
        /// </summary>
        [FromQuery]
        [JsonProperty("$skip")]
        [ModelBinder(Name = "$skip")]
        public int? Skip { get; set; }
    }
}