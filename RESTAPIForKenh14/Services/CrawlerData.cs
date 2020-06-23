using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Quartz;
using RESTAPIForKenh14.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RESTAPIForKenh14.Services
{
    public class CrawlerData : IJob
    {
        private static readonly string SchedulingStatus = ConfigurationManager.AppSettings["ExecuteStatus"];

        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                if (SchedulingStatus.Equals("ON"))
                {
                    try
                    {
                        HtmlWeb htmlWeb = new HtmlWeb()
                        {
                            AutoDetectEncoding = false,
                            OverrideEncoding = Encoding.UTF8 // Set UTF8 to dsiplay vietnamese  
                        };

                        // Crawler data from Kenh14 website
                        CrawlDataFromKenh14(htmlWeb);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            });

            return task;
        }

        // Crawler data from Kenh14 website
        public static void CrawlDataFromKenh14(HtmlWeb htmlWeb)
        {
            // Load web, store data into document
            HtmlDocument document = htmlWeb.Load("https://kenh14.vn/");

            // Get news list in web
            var itemInDIVList = document.DocumentNode.QuerySelectorAll("ul.knsw-list > div > li").ToList();
            var itemNotInDIVList = document.DocumentNode.QuerySelectorAll("ul.knsw-list > li").ToList();


            foreach (var item in itemInDIVList)
            {
                var videoLinkNode = item.QuerySelector("div.knswli-left > a");

                if (videoLinkNode != null)
                {
                    // Get link to detail, title
                    var LinkDetail = videoLinkNode.Attributes["href"].Value.Trim();
                    var Title = HttpUtility.HtmlDecode(videoLinkNode.Attributes["title"].Value.Trim());

                    // Get category of news
                    var categoryNode = item.QuerySelector("div.knswli-meta > a");
                    var Category = categoryNode.InnerText.Trim();

                    // Add news into DB
                    News record = new News();
                    record.Title = Title;
                    record.ContentNews = Category;
                    record.URL = LinkDetail;
                    record.PostTime = DateTime.Now;

                    using (NewsDBContext dbContext = new NewsDBContext())
                    {
                        dbContext.News.Add(record);
                        dbContext.SaveChanges();
                    }
                }
            }

            foreach (var item in itemNotInDIVList)
            {
                // Get link to detail, title
                var linkNode = item.QuerySelector("div.knswli-right > h3 > a");

                if (linkNode != null)
                {
                    var LinkDetail = linkNode.Attributes["href"].Value.Trim();
                    var Title = HttpUtility.HtmlDecode(linkNode.InnerText.Trim());

                    // Get category of news
                    var categoryNode = item.QuerySelector("div.knswli-right > div > a");

                    var Category = "";

                    if (categoryNode != null)
                    {
                        Category = categoryNode.InnerText.Trim();
                    }

                    News record = new News();
                    record.Title = Title;
                    record.ContentNews = Category;
                    record.URL = LinkDetail;
                    record.PostTime = DateTime.Now;

                    using (NewsDBContext dbContext = new NewsDBContext())
                    {
                        dbContext.News.Add(record);
                        dbContext.SaveChanges();
                    }
                }
            }
        }
    }
}