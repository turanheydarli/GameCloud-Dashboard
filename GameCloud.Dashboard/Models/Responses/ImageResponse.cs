namespace GameCloud.Dashboard.Models.Responses;

public record ImageResponse(
    Guid Id,
    string Url,
    int Width,
    int Height,
    string FileType,
    string StorageProvider,
    ImageType Type,
    List<ImageVariant> Variants,
    DateTime CreatedAt);

public enum ImageType
{
    GameIcon,
    DeveloperProfile
}

public class ImageVariant
{
    public string Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string VariantType { get; set; }
    public Guid ImageDocumentId { get; set; }
}