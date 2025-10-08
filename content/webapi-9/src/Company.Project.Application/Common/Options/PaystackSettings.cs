namespace Company.Project.Application.Common.Options;

public struct PaystackSettings
{
    public string PublicKey { get; set; }
    public string SecretKey { get; set; }
    public string BaseUrl { get; set; }
    public bool IsEnabled { get; set; }
}
