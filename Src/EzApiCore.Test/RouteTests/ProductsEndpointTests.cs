using System;
using System.Net.Http;
using System.Threading.Tasks;
using EzApiCore.Api;
using EzApiCore.Test;
using Newtonsoft.Json;
using Xunit;

namespace EzApiCore.RouteTests
{
    public class ProductsEndpointTests : IClassFixture<TestFixture<Startup>>
    {
        private HttpClient Client;

        public ProductsEndpointTests(TestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        [Fact]
        public async Task ProductsAsyncTest()
        {
            var request = "/odata/Productss?$top=2";
            var response = await Client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
        }
    }
}