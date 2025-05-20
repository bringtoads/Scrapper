using HtmlAgilityPack;
using Scrapper.Data;

namespace Scrapper.Helper
{
    public static class ImageUtility
    {
        public static async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(imageUrl);
        }
    }
}
