namespace TenantService.Application;

public class DataFoldersOptions
{
    public string BasePath { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string DocsPath { get; set; } = string.Empty;

    public string ImageDirectory => Path.Combine(BasePath, ImagePath);
    public string DocsDirectory => Path.Combine(BasePath, DocsPath);
}