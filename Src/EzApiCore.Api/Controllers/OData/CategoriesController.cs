using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URF.Core.Abstractions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using EzApiCore.Data.Models;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace EzApiCore.Api.OData
{
    [Route("~/odata/[Controller]")]
    public class CategoriesController : ODataController
    {
        private readonly ICategoriesService _categoriesService;
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(
            ICategoriesService categoriesService,
            IUnitOfWork unitOfWork
        )
        {
            _categoriesService = categoriesService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("")]
        public IEnumerable<Categories> GetAll()
        {
            return _categoriesService.Queryable();
        }

        [HttpGet("{key}")]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 500)]
        public async Task<IActionResult> GetItem(int key)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categories = await _categoriesService.FindAsync(key);

            if (categories == null)
                return NotFound();

            return Ok(categories);
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> PutItem(int key, [FromBody] Categories categories)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (key != categories.CategoryId)
                return BadRequest();
            
            _categoriesService.Update(categories);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _categoriesService.ExistsAsync(key))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost("")]
        public async Task<IActionResult> PostItem([FromBody] Categories categories)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _categoriesService.Insert(categories);

            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction("Get", new { key = categories.CategoryId }, categories);
        }

        /*         [AcceptVerbs("PATCH")]
        */
    [HttpPatch("{key}")]
        public async Task<IActionResult> PatchItem([int key, [FromBody] Delta<Categories> categories)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _categoriesService.FindAsync(key);
            if (entity == null)
                return NotFound();

            categories.Patch(entity);
            _categoriesService.Update(entity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _categoriesService.ExistsAsync(key))
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

            var result = await _categoriesService.DeleteAsync(key);

            if (!result)
                return NotFound();

            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}
