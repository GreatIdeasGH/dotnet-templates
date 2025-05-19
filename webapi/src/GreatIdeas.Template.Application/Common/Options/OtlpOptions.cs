namespace GreatIdeas.Template.Application.Common.Options;

public struct OtlpOptions
{
    public required string Endpoint { get; set; }
    public string? GrafanaLokiEndpoint { get; set; }
    public string? PrometheusEndpoint { get; set; }
    public string? GrafanaAgentEndpoint { get; set; }
    public string? ZipkinEndpoint { get; set; }
    public string? JaegerEndpoint { get; set; }
    public required string SeqEndpoint { get; set; }
}
