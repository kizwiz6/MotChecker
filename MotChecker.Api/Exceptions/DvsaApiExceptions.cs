using System.Net;

namespace MotChecker.Api.Exceptions;

public class DvsaApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public DvsaApiException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        StatusCode = statusCode;
    }
}