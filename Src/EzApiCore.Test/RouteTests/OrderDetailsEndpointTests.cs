using System;
using System.Net.Http;
using System.Threading.Tasks;
using EzApiCore.Api;
using EzApiCore.Test;
using Newtonsoft.Json;
using Xunit;

namespace EzApiCore.RouteTests
{
    public class CustomersTests : IClassFixture<TestFixture<Startup>>
    {
        private HttpClient Client;

        public CustomersTests(TestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        [Fact]
        public async Task CustomersAsyncTest()
        {
            var request = "/odata/Customers?$top=2";
            var response = await Client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
        }
    }
}