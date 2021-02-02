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
    public class CustomersController : ODataController
    {
        private readonly ICustomerService _customersService;
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(
            ICustomerService customersService,
            IUnitOfWork unitOfWork
        )
        {
            _customersService = customersService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("")]
        public IEnumerable<Customers> GetAll()
        {
            return _customersService.Queryable();
        }

        [HttpGet("{key}")]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 500)]
        public async Task<IActionResult> GetItem(string key)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customers = await _customersService.FindAsync(key);

            if (customers == null)
                return NotFound();

            return Ok(customers);
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> PutItem(string key, [FromBody] Customers customers)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (key != customers.CustomerId)
                return BadRequest();

            _customersService.Update(customers);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _customersService.ExistsAsync(key))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost("")]
        public async Task<IActionResult> PostItem([FromBody] Customers customers)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _customersService.Insert(customers);

            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction("Get", new { key = customers.CustomerId }, customers);
        }

        /*         [AcceptVerbs("PATCH")]
        */
        [HttpPatch("{key}")]
        public async Task<IActionResult> PatchItem(string key, [FromBody] Delta<Customers> customers)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _customersService.FindAsync(key);
            if (entity == null)
                return NotFound();

            customers.Patch(entity);
            _customersService.Update(entity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _customersService.ExistsAsync(key))
                    return NotFound();
                throw;
            }

            return Updated(entity);
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteItem(string key)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customersService.DeleteAsync(key);

            if (!result)
                return NotFound();

            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}
