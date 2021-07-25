using MediatR;
using Microsoft.AspNetCore.Mvc;
using Outhink.RequestModels.QueryRequestModels;
using System;
using System.Threading.Tasks;

namespace Outhink.Controllers
{
    /// <summary>
    /// Controller used to manage the order operations
    /// </summary>
    [Route("api/items")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Controller Constructor
        /// </summary>
        /// <param name="mediator">Mediator DI</param>
        public ItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// API used to get a list of items
        /// </summary>
        /// <param name="requestModel">Request Model Used</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetItems([FromQuery] GetItemsRequestModel requestModel)
        {
            try
            {
                var response = await _mediator.Send(requestModel);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}
