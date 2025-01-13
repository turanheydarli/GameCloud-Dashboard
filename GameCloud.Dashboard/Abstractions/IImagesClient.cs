using Refit;

namespace GameCloud.Dashboard.Abstractions;

public interface IImagesClient
{
    [Get("/api/v1/images/{id}")]
    Task<Stream> GetImageAsync(Guid id, string? variant = null);

    [Get("/api/v1/images/{fullPath}")]
    Task<Stream> GetImageByPathAsync(string fullPath);
}