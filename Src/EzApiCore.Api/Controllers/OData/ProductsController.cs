using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData;
using URF.Core.Abstractions;
using EzApiCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using EzApiCore.Service;

namespace EzApiCore.Api.OData
{
    [Route("~/odata/[Controller]")]
    public class ProductsController : ODataController
    {
        private readonly IProductService _productsService;
        private readonly IUnitOfWork _unitOfWork;

        public ProductsController(
            IProductService productsService,
            IUnitOfWork unitOfWork
        )
        {
            _productsService = productsService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("")]
        public IEnumerable<Products> GetAll()
        {
            return _productsService.Queryable();
        }

        [HttpGet("{key}")]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 500)]
        public async Task<IActionResult> GetItem(int key)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var products = await _productsService.FindAsync(key);

            if (products == null)
                return NotFound();

            return Ok(products);
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> PutItem(int key, [FromBody] Products products)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (key != products.ProductId)
                return BadRequest();

            _productsService.Update(products);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _productsService.ExistsAsync(key))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost("")]
        public async Task<IActionResult> PostItem([FromBody] Products products)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _productsService.Insert(products);

            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction("Get", new { key = products.ProductId }, products);
        }

        /*         [AcceptVerbs("PATCH")]
        */
        [HttpPatch("{key}")]
        public async Task<IActionResult> PatchItem(int key, [FromBody] Delta<Products> products)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _productsService.FindAsync(key);
            if (entity == null)
                return NotFound();

            products.Patch(entity);
            _productsService.Update(entity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _productsService.ExistsAsync(key))
                    return NotFound();
                throw;
            }

            return Updated(entity);
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteItem(int key)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _productsService.DeleteAsync(key);

            if (!result)
                return NotFound();

            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}
