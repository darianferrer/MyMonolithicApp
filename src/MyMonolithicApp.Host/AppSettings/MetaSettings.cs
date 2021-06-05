using Microsoft.OpenApi.Models;

namespace MyMonolithicApp.Host.AppSettings
{
    public class MetaSettings
    {
        public string Name { get; set; } = null!;
        public string Version { get; set; } = null!;
        public string? Description { get; set; }
        public OpenApiContact? Contact { get; set; }
    }
}
