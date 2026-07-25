using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartInvoice.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations CLI.
/// Used when running: dotnet ef migrations add/update from the API project.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=smartinvoice;Username=smartinvoice;Password=SmartInvoice@Dev123");

        return new AppDbContext(optionsBuilder.Options);
    }
}
