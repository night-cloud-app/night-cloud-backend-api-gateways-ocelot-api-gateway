namespace OcelotApiGateway.DataTransferObjects;

public class OcelotPath
{
    public string UpstreamPathTemplate { get; set; }
    public string[] UpstreamHttpMethod { get; set; }
    public string DownstreamPathTemplate { get; set; }
    public string DownstreamScheme { get; set; }
    public bool? Protected { get; set; }
}
