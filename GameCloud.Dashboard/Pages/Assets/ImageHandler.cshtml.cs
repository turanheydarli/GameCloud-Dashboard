using System.Net.Mime;
using GameCloud.Dashboard.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameCloud.Dashboard.Pages.Assets;

public class ImageHandlerModel(IImagesClient imagesClient) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string path)
    {
        string decodedPath = Uri.UnescapeDataString(path);
        var stream = await imagesClient.GetImageByPathAsync(decodedPath);
        var contentType = GetContentType(decodedPath);

        return File(stream, contentType);
    }

    private string GetContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();

        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".gif":
                return "image/gif";
            default:
                return MediaTypeNames.Application.Octet;
        }
    }
}