using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URF.Core.Abstractions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using EzApiCore.Data.Models;
using System.Linq;
using EzApiCore.Service;
using System.Net;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace EzApiCore.Api.OData
{
    [ODataRoutePrefix("Products")]
    [ApiController]
    [Route("Products")]
    public class ProductsController : ODataController
    {
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;

        public ProductsController(
            IProductService productService,
            IUnitOfWork unitOfWork)
        {
            _productService = productService;
            _unitOfWork = unitOfWork;
        }

        // e.g. GET odata/Products?$skip=2&$top=10
        [HttpGet]
        [ODataRoute]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 500)]
        public IQueryable<Products> GetAll() => _productService.Queryable();

        [HttpGet]
        [ODataRoute("({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 500)]
        // e.g.  GET odata/Products(37)
        public async Task<IActionResult> GetItem([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.FindAsync(key);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // e.g. PUT odata/Products(37)
        [HttpPut]
        [ODataRoute("({key})")]
        public async Task<IActionResult> PutItem([FromODataUri] int key, [FromBody] Products products)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (key != products.ProductId)
                return BadRequest();

            _productService.Update(products);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _productService.ExistsAsync(key))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        [ODataRoute]
        public async Task<IActionResult> PostItem([FromBody] Products products)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _productService.Insert(products);
            await _unitOfWork.SaveChangesAsync();

            return Created(products);
        }

        [HttpPatch]
        [AcceptVerbs("PATCH")]
        [ODataRoute("({key})")]
        public async Task<IActionResult> PatchItem([FromODataUri] int key, [FromBody] Delta<Products> product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _productService.FindAsync(key);
            if (entity == null)
                return NotFound();

            product.Patch(entity);
            _productService.Update(entity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _productService.ExistsAsync(key))
                    return NotFound();
                throw;
            }
            return Updated(entity);
        }

        [HttpDelete]
        [ODataRoute]
        public async Task<IActionResult> DeleteItem([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _productService.DeleteAsync(key);

            if (!result)
                return NotFound();

            await _unitOfWork.SaveChangesAsync();

            return StatusCode((int) HttpStatusCode.NoContent);
        }
    }
}