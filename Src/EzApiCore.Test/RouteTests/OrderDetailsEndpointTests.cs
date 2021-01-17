using System;
using System.Net.Http;
using System.Threading.Tasks;
using EzApiCore.Api;
using EzApiCore.Test;
using Newtonsoft.Json;
using Xunit;

namespace EzApiCore.RouteTests
{
    public class OrderDetailsEndpointTests : IClassFixture<TestFixture<Startup>>
    {
        private HttpClient Client;

        public OrderDetailsEndpointTests(TestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        [Fact]
        public async Task OrderDetailsAsyncTest()
        {
            var request = "/odata/OrderDetailss?$top=2";
            var response = await Client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
        }
    }
}