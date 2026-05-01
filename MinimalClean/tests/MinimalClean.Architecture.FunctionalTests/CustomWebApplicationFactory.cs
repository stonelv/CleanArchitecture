using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MinimalClean.Architecture.Web.Infrastructure;
using MinimalClean.Architecture.Web.Infrastructure.Data;

namespace MinimalClean.Architecture.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.UseEnvironment("Testing");
    var host = builder.Build();
    host.Start();

    var serviceProvider = host.Services;

    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<AppDbContext>();

      var logger = scopedServices
          .GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      db.Database.EnsureCreated();

      try
      {
        SeedData.PopulateTestDataAsync(db, logger).GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the database. Error: {Message}", ex.Message);
      }
    }

    return host;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder
        .ConfigureServices(services =>
        {
          var dbContextDescriptors = services.Where(
            d => d.ServiceType == typeof(AppDbContext) ||
                 d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();

          foreach (var descriptor in dbContextDescriptors)
          {
            services.Remove(descriptor);
          }

          string inMemoryCollectionName = Guid.NewGuid().ToString();

          services.AddDbContext<AppDbContext>(options =>
          {
            options.UseInMemoryDatabase(inMemoryCollectionName);
          });

          services.AddMediator(options =>
          {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies =
            [
              typeof(InfrastructureServiceExtensions).Assembly
            ];
          });
        });
  }
}
