using System;

namespace app;

public class AppSettings
{
    public const string CONFIG = "S3Demo";
    public string ServiceUrl { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string CatPicture { get; set; }
    public int MaxPageContinuations { get; set; }
}