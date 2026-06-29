using BeaverX.Core.Dependency;

using BeaverX.Domain.Repositories;

using Blog.MVC.Helpers;

using Blog.MVC.Models.Blog;

using Blog.MVC.Models.Users;

using Blog.MVC.Services.Users;

using Microsoft.EntityFrameworkCore;



namespace Blog.MVC.Data;



public class BlogDbSeeder : IScopedDependency

{

    private readonly BlogDbContext _dbContext;

    private readonly IRepository<AppUser> _userRepository;

    private readonly IRepository<Category> _categoryRepository;

    private readonly IRepository<Tag> _tagRepository;

    private readonly IRepository<Article> _articleRepository;

    private readonly IPasswordHasher _passwordHasher;



    public BlogDbSeeder(

        BlogDbContext dbContext,

        IRepository<AppUser> userRepository,

        IRepository<Category> categoryRepository,

        IRepository<Tag> tagRepository,

        IRepository<Article> articleRepository,

        IPasswordHasher passwordHasher)

    {

        _dbContext = dbContext;

        _userRepository = userRepository;

        _categoryRepository = categoryRepository;

        _tagRepository = tagRepository;

        _articleRepository = articleRepository;

        _passwordHasher = passwordHasher;

    }



    public async Task SeedAsync(CancellationToken cancellationToken = default)

    {

        await _dbContext.Database.MigrateAsync(cancellationToken);



        await SeedUsersAsync(cancellationToken);

        await SeedCategoriesAsync(cancellationToken);

        await SeedTagsAsync(cancellationToken);

        await SeedSampleArticleAsync(cancellationToken);

    }



    private async Task SeedUsersAsync(CancellationToken cancellationToken)

    {

        if ((await _userRepository.GetListAsync(cancellationToken: cancellationToken)).Count > 0)

        {

            return;

        }



        await _userRepository.InsertAsync(new AppUser

        {

            UserName = "admin",

            Email = "admin@blog.local",

            DisplayName = "管理员",

            Role = UserRole.Admin,

            IsActive = true,

            PasswordHash = _passwordHasher.HashPassword("Admin@123")

        }, cancellationToken: cancellationToken);



        await _userRepository.InsertAsync(new AppUser

        {

            UserName = "author",

            Email = "author@blog.local",

            DisplayName = "博主",

            Role = UserRole.Author,

            IsActive = true,

            PasswordHash = _passwordHasher.HashPassword("Author@123")

        }, cancellationToken: cancellationToken);

    }



    private async Task SeedCategoriesAsync(CancellationToken cancellationToken)

    {

        if ((await _categoryRepository.GetListAsync(cancellationToken: cancellationToken)).Count > 0)

        {

            return;

        }



        var categories = new[]

        {

            new Category { Name = "技术", Slug = "tech", Description = "编程与开发相关", SortOrder = 1 },

            new Category { Name = "生活", Slug = "life", Description = "日常生活记录", SortOrder = 2 },

            new Category { Name = "随笔", Slug = "essay", Description = "随想与杂谈", SortOrder = 3 }

        };



        foreach (var category in categories)

        {

            await _categoryRepository.InsertAsync(category, cancellationToken: cancellationToken);

        }

    }



    private async Task SeedTagsAsync(CancellationToken cancellationToken)

    {

        if ((await _tagRepository.GetListAsync(cancellationToken: cancellationToken)).Count > 0)

        {

            return;

        }



        var tags = new[] { ".NET", "ASP.NET Core", "MySQL", "Bootstrap", "MVC" };

        foreach (var name in tags)

        {

            await _tagRepository.InsertAsync(new Tag

            {

                Name = name,

                Slug = SlugHelper.Generate(name, "tag")

            }, cancellationToken: cancellationToken);

        }

    }



    private async Task SeedSampleArticleAsync(CancellationToken cancellationToken)

    {

        if ((await _articleRepository.GetListAsync(cancellationToken: cancellationToken)).Count > 0)

        {

            return;

        }



        var author = await _userRepository.FindAsync(x => x.UserName == "author", cancellationToken: cancellationToken);

        var category = await _categoryRepository.FindAsync(x => x.Slug == "tech", cancellationToken: cancellationToken);

        var tags = await _tagRepository.GetListAsync(cancellationToken: cancellationToken);



        if (author == null || category == null || tags.Count == 0)

        {

            return;

        }



        var article = new Article

        {

            Title = "欢迎使用个人博客系统",

            Slug = "welcome-to-blog",

            Summary = "这是一篇示例文章，介绍如何使用后台发布富文本内容。",

            Content = "## 你好，博客！\n\n你可以在后台使用 **Editor.md** 撰写 Markdown 文章，并管理分类与标签。\n\n- 支持 Markdown 语法\n- 支持图片上传（MinIO）\n- 支持分类与多标签",

            CategoryId = category.Id,

            AuthorId = author.Id,

            Status = ArticleStatus.Published,

            PublishedTime = DateTime.UtcNow

        };



        await _articleRepository.InsertAsync(article, cancellationToken: cancellationToken);



        var selectedTags = tags.Take(3).ToList();

        foreach (var tag in selectedTags)

        {

            _dbContext.ArticleTags.Add(new ArticleTag

            {

                ArticleId = article.Id,

                TagId = tag.Id

            });

        }



        await _dbContext.SaveChangesAsync(cancellationToken);

    }

}


