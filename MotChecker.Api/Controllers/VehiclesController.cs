using Microsoft.AspNetCore.Mvc;
using MotChecker.Api.Services;
using MotChecker.Models;

namespace MotChecker.Api.Controllers
{
    // VehiclesController.cs
    /// <summary>
    /// API Controller for handling vehicle information requests
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly DvsaApiProxy _dvsaApi;

        /// <summary>
        /// Initialises a new instance of the VehiclesController
        /// </summary>
        /// <param name="dvsaApi">DVSA API proxy service</param>
        public VehiclesController(DvsaApiProxy dvsaApi)
        {
            _dvsaApi = dvsaApi;
        }

        /// <summary>
        /// Retrieves vehicle details by registration number
        /// </summary>
        /// <param name="registration">Vehicle registration number</param>
        /// <returns>Vehicle details if found</returns>
        /// <response code="200">Returns the vehicle details</response>
        /// <response code="500">If an error occurs during processing</response>
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
