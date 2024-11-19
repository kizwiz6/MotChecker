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
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(DvsaApiProxy dvsaApi, ILogger<VehiclesController> logger)
        {
            _dvsaApi = dvsaApi;
            _logger = logger;
        }

        [HttpGet("{registration}")]
        public async Task<ActionResult<VehicleDetails>> Get(string registration)
        {
            try
            {
                _logger.LogInformation("Received request for registration: {Registration}", registration);
                var result = await _dvsaApi.GetVehicleDetailsAsync(registration);
                _logger.LogInformation("Successfully retrieved details for {Registration}", registration);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request for {Registration}", registration);
                return StatusCode(500, new { error = "Unable to retrieve vehicle details" });
            }
        }
    }
}
