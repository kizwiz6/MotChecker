using Microsoft.AspNetCore.Mvc;
using MotChecker.Api.Services;
using MotChecker.Models;

namespace MotChecker.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly DvsaApiProxy _dvsaApi;

        public VehiclesController(DvsaApiProxy dvsaApi)
        {
            _dvsaApi = dvsaApi;
        }

        [HttpGet("{registration}")]
        public async Task<ActionResult<VehicleDetails>> Get(string registration)
        {
            try
            {
                return await _dvsaApi.GetVehicleDetailsAsync(registration);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
