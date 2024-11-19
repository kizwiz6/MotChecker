using Microsoft.AspNetCore.Mvc;
using MotChecker.Api.Services;
using MotChecker.Models;

namespace MotChecker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly DvsaApiProxy _dvsaApi;

        public VehiclesController(DvsaApiProxy dvsaApi)
        {
            _dvsaApi = dvsaApi;
        }

        [HttpGet("{registration}")]
        [Produces("application/json")]
        public async Task<ActionResult<VehicleDetails>> Get(string registration)
        {
            try
            {
                var result = await _dvsaApi.GetVehicleDetailsAsync(registration);
                return Ok(result);
            }
            catch (Exception ex)
            {
                 return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
