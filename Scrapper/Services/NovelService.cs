using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scrapper.Data.Configs;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Helper;
using Scrapper.Interfaces;
using System.Net;

namespace Scrapper.Services
{
    internal class NovelService : INovelService
    {
        private readonly NovelApiClient _apiClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly NovelSettings _settings;
        private readonly ILogger<NovelService> _logger;

        public NovelService(NovelApiClient apiClient, IUnitOfWork unitOfWork, IOptions<NovelSettings> options, ILogger<NovelService> logger)
        {
            _apiClient = apiClient;
            _unitOfWork = unitOfWork;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<IEnumerable<Novel>> GetAllNovelDetails()
        {
            return await _unitOfWork.NovelRepository.GetAllAsync();
        }

        public async Task ScrapeLatest()
        {
            var html = await _apiClient.GetLatest();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var lastPageNode = doc.DocumentNode.SelectSingleNode("//li[normalize-space(@class)='last']/a");
            int maxPage = 0;

            if (lastPageNode is not null)
            {
                var lastPageUrl = WebUtility.HtmlDecode(lastPageNode.GetAttributeValue("href", ""));
                var uri = new Uri(lastPageUrl);
                var queryParams = QueryHelpers.ParseQuery(uri.Query);

                if (queryParams.TryGetValue("page", out var pageValues) &&
                    int.TryParse(pageValues.ToString(), out int pageNumber))
                {
                    maxPage = pageNumber;
                }
            }

            for (int i = 0; i <= maxPage; i++)
            {
                await ScrapeRows(i);
            }
        }

        private async Task ScrapeRows(int pageNum)
        {
            var html = await _apiClient.Get(_settings.NavLatest + pageNum);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rows = doc.DocumentNode.SelectNodes("//div[@class='list list-novel col-xs-12']/div[@class='row']");

            if (rows is null)
            {
                _logger.LogWarning("No rows found on page {Page}", pageNum);
                return;
            }

            foreach (var row in rows)
            {
                var titleNode = row.SelectSingleNode(".//div[@class='col-xs-7']//h3[@class='novel-title']/a");
                var authorNode = row.SelectSingleNode(".//div[@class='col-xs-7']//span[@class='author']");

                if (titleNode is null || authorNode is null)
                    continue;

                var title = WebUtility.HtmlDecode(titleNode.InnerText.Trim());
                var authorName = WebUtility.HtmlDecode(authorNode.InnerText.Replace("glyphicon-pencil", "").Trim());
                var sourceUrl = titleNode.GetAttributeValue("href", "");
                var description = await ScrapeNovelDescription(sourceUrl);

                await SaveNovelAndAuthorAsync(title, authorName, sourceUrl,description);
            }
        }
        public async Task<String> ScrapeNovelDescription(string sourceUrl)
        {
            var html = await _apiClient.Get(sourceUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var descDiv = doc.DocumentNode.SelectSingleNode("//div[@class='desc-text']");
            if (descDiv is not null)
            {
                var description = descDiv.InnerHtml;
                return description;
            }
            return string.Empty;
        }
        private async Task SaveNovelAndAuthorAsync(string novelTitle, string authorName, string novelSourceUrl,string description)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var author = await _unitOfWork.AuthorRepository.GetByNameAsync(authorName);
                if (author == null)
                {
                    author = new Author { Name = authorName };
                    await _unitOfWork.AuthorRepository.AddAsync(author);
                    await _unitOfWork.CompleteAsync();
                }

                var existingNovel = await _unitOfWork.NovelRepository.GetBySourceUrlAsync(novelSourceUrl);
                if (existingNovel == null)
                {
                    var novel = new Novel
                    {
                        Title = novelTitle,
                        SourceUrl = novelSourceUrl,
                        Description =description,
                        AuthorId = author.AuthorId
                    };

                    await _unitOfWork.NovelRepository.AddAsync(novel);
                    await _unitOfWork.CompleteAsync();

                    _logger.LogInformation("Saved Novel: {Title} by {Author}", novelTitle, authorName);
                }
                else
                {
                    _logger.LogInformation("Skipping duplicate novel: {Title}", novelTitle);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error saving novel: {Title} by {Author}", novelTitle, authorName);
            }
        }
    }
}

