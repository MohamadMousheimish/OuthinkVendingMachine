using MediatR;
using Microsoft.AspNetCore.Mvc;
using Outhink.RequestModels.CommandRequestModels;
using Outhink.RequestModels.QueryRequestModels;
using System;
using System.Threading.Tasks;

namespace Outhink.Controllers
{
    /// <summary>
    /// Controller used to manage the coins operations
    /// </summary>
    [Route("api/coins")]
    [ApiController]
    public class CoinsController : ControllerBase
    {

        private readonly IMediator _mediator;

        /// <summary>
        /// Controller Constructor
        /// </summary>
        /// <param name="mediator">Mediator DI</param>
        public CoinsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// API used to get the vending machine coins
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetVendingMachineCoins([FromQuery] GetCoinsRequestModel requestModel)
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
        /// API used to insert a coin into the vending machine
        /// </summary>
        /// <remarks>This API will only store the inserted coin into the memory cache</remarks>
        /// <param name="requestModel">Request Model Used</param>
        /// <returns></returns>
        [HttpPost("insert-coin")]
        public async Task<IActionResult> InsertCoins([FromBody] InsertCoinsRequestModel requestModel)
        {
            try
            {
                var response = await _mediator.Send(requestModel);
                if (response.Succeeded)
                {
                    return Ok();
                }
                return BadRequest("Unable to insert coins");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
