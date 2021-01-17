using System;
using System.Net.Http;
using System.Threading.Tasks;
using EzApiCore.Api;
using EzApiCore.Test;
using Newtonsoft.Json;
using Xunit;

namespace EzApiCore.RouteTests
{
    public class CustomersEndpointTests : IClassFixture<TestFixture<Startup>>
    {
        private HttpClient Client;

        public CustomersEndpointTests(TestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        [Fact]
        public async Task CustomersAsyncTest()
        {
            var request = "/odata/Customerss?$top=2";
            var response = await Client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
        }
    }
}