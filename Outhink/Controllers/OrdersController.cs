using MediatR;
using Microsoft.AspNetCore.Mvc;
using Outhink.RequestModels.CommandRequestModels;
using System;
using System.Threading.Tasks;

namespace Outhink.Controllers
{
    /// <summary>
    /// Controller used to manage the order operations
    /// </summary>
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Controller Constructor
        /// </summary>
        /// <param name="mediator">Mediator DI</param>
        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// API used to cancel the order of the user
        /// </summary>
        /// <remarks>Canceling order will only clean the cache</remarks>
        /// <returns></returns>
        [HttpPost("cancel-order")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderRequestModel requestModel)
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
        
        /// <summary>
        /// API used to buy a specific order
        /// </summary>
        /// <param name="requestModel">Request Model Used</param>
        /// <returns></returns>
        [HttpPost("buy")]
        public async Task<IActionResult> BuyOrder([FromBody] MakeOrderRequestModel requestModel)
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
