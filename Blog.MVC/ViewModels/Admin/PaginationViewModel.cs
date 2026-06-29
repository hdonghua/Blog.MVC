namespace Blog.MVC.ViewModels.Admin;

public class PaginationViewModel
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }

    public bool HasPrevious { get; init; }

    public bool HasNext { get; init; }

    public string Action { get; init; } = "Index";

    public string? Controller { get; init; }

    public string? Area { get; init; }

    public static PaginationViewModel From<T>(Models.Common.PagedResult<T> result, string? controller = null, string? area = "Admin")
    {
        return new PaginationViewModel
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages,
            HasPrevious = result.HasPrevious,
            HasNext = result.HasNext,
            Controller = controller,
            Area = area
        };
    }
}
