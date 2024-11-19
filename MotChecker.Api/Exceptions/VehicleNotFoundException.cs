using System.Net;

namespace MotChecker.Api.Exceptions;

public class VehicleNotFoundException : DvsaApiException
{
    public string Registration { get; }

    public VehicleNotFoundException(string registration)
        : base($"Vehicle with registration {registration} not found", HttpStatusCode.NotFound)
    {
        Registration = registration;
    }
}