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
    [ODataRoutePrefix("OrderDetails")]
    [ApiController]
    [Route("OrderDetails")]
    public class OrderDetailsController : ODataController
    {
        private readonly IOrderDetailService _orderDetailService;
        private readonly IUnitOfWork _unitOfWork;

        public OrderDetailsController(
            IOrderDetailService orderDetailService,
            IUnitOfWork unitOfWork)
        {
            _orderDetailService = orderDetailService;
            _unitOfWork = unitOfWork;
        }

        // e.g. GET odata/Products?$skip=2&$top=10
        [HttpGet]
        [ODataRoute]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 500)]
        public IQueryable<OrderDetails> GetAll()
        {
            return _orderDetailService.Queryable();
        }

        // e.g.  GET odata/Products(37)
        [HttpGet]
        [ODataRoute("({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxNodeCount = 500)]
        public async Task<IActionResult> GetItem([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _orderDetailService.FindAsync(key);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // e.g. PUT odata/Products(37)
        [HttpPut]
        [ODataRoute("({key})")]
        public async Task<IActionResult> PutItem([FromODataUri] int key, [FromBody] OrderDetails orderDetail)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (key != orderDetail.ProductId)
                return BadRequest();

            _orderDetailService.Update(orderDetail);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _orderDetailService.ExistsAsync(key))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        [ODataRoute]
        public async Task<IActionResult> PostItem([FromBody] OrderDetails orderDetails)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _orderDetailService.Insert(orderDetails);
            await _unitOfWork.SaveChangesAsync();

            return Created(orderDetails);
        }

        [HttpPatch]
        [AcceptVerbs("PATCH")]
        [ODataRoute("({key})")]
        public async Task<IActionResult> PatchItem([FromODataUri] int key, [FromBody] Delta<OrderDetails> orderDetail)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _orderDetailService.FindAsync(key);
            if (entity == null)
                return NotFound();

            orderDetail.Patch(entity);
            _orderDetailService.Update(entity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _orderDetailService.ExistsAsync(key))
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

            var result = await _orderDetailService.DeleteAsync(key);

            if (!result)
                return NotFound();

            await _unitOfWork.SaveChangesAsync();

            return StatusCode((int) HttpStatusCode.NoContent);
        }
    }
}