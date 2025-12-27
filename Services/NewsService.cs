using System.Text.Json;
using MvcApp.Models.News;

namespace MvcApp.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public NewsService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<NewsArticle>> GetTopHeadlinesAsync(string category = "general")
        {
            var apiKey = _config["NewsApi:ApiKey"];

            var url = $"https://newsapi.org/v2/top-headlines" +
                      $"?country=us" +
                      $"&category={category}" +
                      $"&pageSize=12" +
                      $"&apiKey={apiKey}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"News API Error: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var articles = doc.RootElement.GetProperty("articles");

            var newsList = new List<NewsArticle>();

            foreach (var item in articles.EnumerateArray())
            {
                newsList.Add(new NewsArticle
                {
                    Title = item.GetProperty("title").GetString() ?? "",
                    Description = item.GetProperty("description").GetString() ?? "",
                    Url = item.GetProperty("url").GetString() ?? "",
                    UrlToImage = item.GetProperty("urlToImage").GetString() ?? "",
                    PublishedAt = item.GetProperty("publishedAt").GetDateTime(),
                    Source = item.GetProperty("source").GetProperty("name").GetString() ?? ""
                });
            }

            return newsList;
        }


    }
}
