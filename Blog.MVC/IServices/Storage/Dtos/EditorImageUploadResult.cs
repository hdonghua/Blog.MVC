namespace Blog.MVC.IServices.Storage.Dtos;

/// <summary>
/// Editor.md 图片上传接口约定的 JSON 结构。
/// </summary>
public class EditorImageUploadResult
{
    public int success { get; set; }

    public string message { get; set; } = null!;

    public string? url { get; set; }
}
