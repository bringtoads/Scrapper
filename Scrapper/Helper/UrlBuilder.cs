namespace Scrapper.Helper
{
    public static class UrlBuilder
    {
        private const string BaseUrl = "https://novelbin.com/b";

        public static string BuildChapterUrl(string novelName, string chapterName)
        {
            return $"{BaseUrl}/{novelName}/{chapterName}";
        }
    }
}
