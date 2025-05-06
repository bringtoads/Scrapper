using Scrapper.Data.Configs;
using Scrapper.Data;
using Scrapper.Helper;
using Scrapper.Manager;
using Scrapper.Services;
using Scrapper.Interfaces;
using Scrapper.Repositories;
using Scrapper.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrapper.Background;
using Scrapper.Services.ScrapperService;
internal class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
           .ConfigureAppConfiguration((context, config) =>
           {
               config.SetBasePath(AppContext.BaseDirectory)
                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
           })
           .ConfigureServices((context, services) =>
           {
               IConfiguration configuration = context.Configuration;

               var connectionString = configuration.GetConnectionString("DefaultConnection");

               services.AddDbContext<AppDbContext>(options =>
                   options.UseNpgsql(connectionString));
               services.Configure<NovelSettings>(configuration.GetSection("NovelSettings"));

               services.AddHttpClient<NovelApiClient>((serviceProvider, client) =>
               {
                   var novelSettings = serviceProvider.GetRequiredService<IOptions<NovelSettings>>().Value;
                   client.BaseAddress = new Uri(novelSettings.Home);
                   client.DefaultRequestHeaders.Add("Accept", "application/json");
               });

               services.AddHostedService<DailyScraperWorker>();
               services.AddScoped<IAuthorRepository, AuthorRepository>();
               services.AddScoped<IChapterRepository, ChapterRepository>();
               services.AddScoped<INovelRepository, NovelRepository>();
               services.AddScoped<IScrapeHistoryRepository, ScrapeHistoryRepository>();
               services.AddScoped<IUnitOfWork, UnitOfWork>();

               services.AddScoped<IChapterService, ChapterService>();
               services.AddScoped<INovelService, NovelService>();
               services.AddScoped<IScrapperService, HtmlAgilityScrapperService>();

               services.AddScoped<INovelManager, NovelManager>();
           })
           .Build();

        var manager = host.Services.GetRequiredService<INovelManager>();
        //await manager.StartScrapingAsync();
        await manager.Start();

        await host.RunAsync(); 

    }

}