namespace Blog.MVC.IServices.Blog.Dtos;

public class CommentDto
{
    public long Id { get; set; }

    public long ArticleId { get; set; }

    public long? ParentId { get; set; }

    public string NickName { get; set; } = null!;

    public string? Website { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public string? ReplyToNickName { get; set; }

    public List<CommentDto> Replies { get; set; } = [];
}

public class CommentAdminDto
{
    public long Id { get; set; }

    public long ArticleId { get; set; }

    public string ArticleTitle { get; set; } = null!;

    public string ArticleSlug { get; set; } = null!;

    public long? ParentId { get; set; }

    public string NickName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Website { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public bool IsApproved { get; set; }
}

public class CreateCommentDto
{
    public long ArticleId { get; set; }

    public long? ParentId { get; set; }

    public string NickName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Website { get; set; }

    public string Content { get; set; } = null!;
}
