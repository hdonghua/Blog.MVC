using BeaverX.EntityFrameworkCore.DependencyInjection;
using Blog.MVC.Data.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Blog.MVC.Data;

public class BlogDbDriverOptionsBuilder : IDbDriverOptionsBuilder
{
    public void Configure<TDbContext>(DbContextOptionsBuilder optionsBuilder, string connectionString)
        where TDbContext : DbContext
    {
        optionsBuilder.UseMySQL(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        });

        optionsBuilder.AddInterceptors(new UtcDateTimeSaveChangesInterceptor());
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }
}
