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
    [ODataRoutePrefix("Customers")]
    [ApiController]
    [Route("Customers")]
    public class CustomersController : ODataController
    {
        private readonly ICustomerService _customerService;
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(
            ICustomerService customerService,
            IUnitOfWork unitOfWork)
        {
            _customerService = customerService;
            _unitOfWork = unitOfWork;
        }

        // e.g. GET odata/Customers?$skip=2&$top=10
        [HttpGet]
        [ODataRoute]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 500)]
        public IQueryable<Customers> GetAll() => _customerService.Queryable();

        // e.g.  GET odata/Customers(37)
        [HttpGet]
        [ODataRoute("({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 500)]
        public async Task<IActionResult> GetItem([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = await _customerService.FindAsync(key);

            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        // e.g. PUT odata/Customers(37)
        [HttpPut]
        [ODataRoute("({key})")]
        public async Task<IActionResult> PutItem([FromODataUri] string key, [FromBody] Customers customer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (key != customer.CustomerId)
                return BadRequest();

            _customerService.Update(customer);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _customerService.ExistsAsync(key))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        [ODataRoute]
        public async Task<IActionResult> PostItem([FromBody] Customers customer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _customerService.Insert(customer);
            await _unitOfWork.SaveChangesAsync();

            return Created(customer);
        }

        [HttpPatch]
        [AcceptVerbs("PATCH")]
        [ODataRoute("({key})")]
        public async Task<IActionResult> PatchItem([FromODataUri] int key, [FromBody] Delta<Customers> customer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _customerService.FindAsync(key);
            if (entity == null)
                return NotFound();

            customer.Patch(entity);
            _customerService.Update(entity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _customerService.ExistsAsync(key))
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

            var result = await _customerService.DeleteAsync(key);

            if (!result)
                return NotFound();

            await _unitOfWork.SaveChangesAsync();

            return StatusCode((int) HttpStatusCode.NoContent);
        }
    }
}