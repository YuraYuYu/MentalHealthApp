using MentalHealthApp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MentalHealthApp.Controllers
{
    public class NewsController : Controller
    {
        private readonly string apiKey = "f6ce0219309143fc9304450cd2a97be2"; // 将此替换为你的API密钥

        public ActionResult GetMentalHealthNews()
        {
            string apiUrl = $"https://newsapi.org/v2/everything?q=Mental+Health&apiKey={apiKey}&pageSize=3";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync(apiUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = response.Content.ReadAsStringAsync().Result;
                        JObject newsData = JObject.Parse(jsonResponse);

                        var articles = new List<NewsArticle>();

                        foreach (var item in newsData["articles"])
                        {
                            articles.Add(new NewsArticle
                            {
                                Title = item["title"]?.ToString() ?? "No Title",
                                Description = item["description"]?.ToString() ?? "No Description",
                                Url = item["url"]?.ToString() ?? "#",
                                UrlToImage = item["urlToImage"]?.ToString() ?? "~/images/default.jpg"
                            });
                        }

                        return View(articles);
                    }
                    else
                    {
                        // 记录错误信息
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        System.Diagnostics.Debug.WriteLine($"API Request Failed: {response.StatusCode}, Content: {errorContent}");
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    // 捕获并记录异常信息
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                    return View("Error");
                }
            }
        }
    }
}