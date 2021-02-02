// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Orders.Api
{
    /// <summary>
    ///     The set of API routes.
    /// </summary>
    public static class Routes
    {
        /// <summary>
        ///     The base API path.
        /// </summary>
        public const string BaseApiPath = "odata";

        /// <summary>
        /// Path to the orders resource.
        /// </summary>
        public const string Orders = BaseApiPath + "/orders";

        /// <summary>
        /// Path to get and order by id.
        /// </summary>
        public const string OrderById = BaseApiPath + "/orders/{id}";
    }
}