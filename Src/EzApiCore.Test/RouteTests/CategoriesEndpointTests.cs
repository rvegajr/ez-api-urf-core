using System;
using System.Net.Http;
using System.Threading.Tasks;
using EzApiCore.Api;
using EzApiCore.Test;
using Newtonsoft.Json;
using Xunit;

namespace EzApiCore.RouteTests
{
    public class CategoriesEndpointTests : IClassFixture<TestFixture<Startup>>
    {
        private HttpClient Client;

        public CategoriesEndpointTests(TestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        [Fact]
        public async Task CategoriesAsyncTest()
        {
            var request = "/odata/Categoriess?$top=2";
            var response = await Client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
        }
    }
}