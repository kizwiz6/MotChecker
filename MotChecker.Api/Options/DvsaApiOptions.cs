using System.ComponentModel.DataAnnotations;

namespace MotChecker.Api.Options;

public class DvsaApiOptions
{
    public const string SectionName = "DvsaApi";

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string ClientSecret { get; init; } = string.Empty;

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required, Url]
    public string ScopeUrl { get; init; } = string.Empty;

    [Required, Url]
    public string TokenUrl { get; init; } = string.Empty;

    [Required, Url]
    public string BaseUrl { get; init; } = string.Empty;
}