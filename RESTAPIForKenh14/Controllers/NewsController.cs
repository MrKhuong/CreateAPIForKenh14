using Newtonsoft.Json;
using RESTAPIForKenh14.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace RESTAPIForKenh14.Controllers
{
    public class NewsController : ApiController
    {
        // Get all news in DB
        [HttpGet]
        public HttpResponseMessage GetAllNews()
        {
            using (NewsDBContext dbContext = new NewsDBContext())
            {
                var allNews = dbContext.News.OrderByDescending(x => x.Id).ToList();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, allNews);
            }
        }

        // Get by id
        [HttpGet]
        public HttpResponseMessage GetByID(int id)
        {
            try
            {
                using (NewsDBContext dBContext = new NewsDBContext())
                {
                    var entity = dBContext.News.FirstOrDefault(x => x.Id == id);

                    if (entity==null)
                    {
                        return Request.CreateErrorResponse(System.Net.HttpStatusCode.NotFound, "News with ID = " + id + " not found");
                    }
                    else
                    {
                        return Request.CreateResponse(System.Net.HttpStatusCode.OK, entity);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, ex);
            }
        }

        // Paging and searching
        [HttpGet]
        public HttpResponseMessage GetNews([FromUri] PagingParameterModel pagingParameterModel)
        {
            using (NewsDBContext dbContext = new NewsDBContext())
            {
                // Return List of news
                var allNews = dbContext.News.OrderByDescending(x => x.Id).ToList();

                // Search parameter with null check
                if (!string.IsNullOrEmpty(pagingParameterModel.QuerySearch))
                {
                    allNews = allNews.Where(a => a.Title.Contains(pagingParameterModel.QuerySearch)).ToList();
                }

                // Get's No of Rows Count
                int count = allNews.Count();

                // Parameter will be passed from query string if it is null then is default Value will be pageNumber:1
                int currentPage = pagingParameterModel.pageNumber;

                // Parameter will be passed from query string if it is null then is default Value will be pageSize:20
                int pageSize = pagingParameterModel.pageSize;

                // Calculating totalPage by dividing (No of Record / pageSize)
                int totalPage = (int)Math.Ceiling(count / (double)pageSize);

                // Return list of news  after apllying paging 
                var items = allNews.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

                // If currentPage is greater than 1 means it has previous page
                var previousPage = currentPage > 1 ? "Yes" : "No";

                // If totalPage is greater than currentPage means it has next page
                var nextPage = currentPage < totalPage ? "Yes" : "No";

                // Object we will send to header
                var paginationMetdata = new
                {
                    TotalCount = count,
                    PageSize = pageSize,
                    CurrentPage = currentPage,
                    TotalPage = totalPage,
                    previousPage,
                    nextPage,
                    querySearch = string.IsNullOrEmpty(pagingParameterModel.QuerySearch) ?
                                  "No parameter passes" : pagingParameterModel.QuerySearch
                };

                // Setting header
                HttpContext.Current.Response.Headers.Add("Paging-Header", JsonConvert.SerializeObject(paginationMetdata));

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, items);
            }
        }

        // POST method
        [HttpPost]
        public HttpResponseMessage CreateANews(News news)
        {
            try
            {
                using (NewsDBContext dBContext = new NewsDBContext())
                {
                    dBContext.News.Add(news);
                    dBContext.SaveChanges();

                    var message = Request.CreateResponse(System.Net.HttpStatusCode.Created, news);
                    message.Headers.Location = new Uri(Request.RequestUri + news.Id.ToString());

                    return message;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, ex);
            }
        }

        // PUT method
        [HttpPut]
        public HttpResponseMessage UpdateANews(int id, [FromBody] News news)
        {
            try
            {
                using (NewsDBContext dBContext = new NewsDBContext())
                {
                    var entity = dBContext.News.FirstOrDefault(x => x.Id == id);

                    if (entity.Id == 0)
                    {
                        return Request.CreateErrorResponse(System.Net.HttpStatusCode.NotFound,
                                                           "News with ID = " + id + "not found");
                    }
                    else
                    {
                        entity.Title = news.Title;
                        entity.ContentNews = news.ContentNews;
                        entity.PostTime = news.PostTime;
                        entity.URL = news.URL;
                        dBContext.SaveChanges();

                        return Request.CreateResponse(System.Net.HttpStatusCode.OK, entity);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, ex);
            }
        }

        // DELETE method
        [HttpDelete]
        public HttpResponseMessage DeleteById(int id)
        {
            try
            {
                using (NewsDBContext dBContext = new NewsDBContext())
                {
                    var entity = dBContext.News.FirstOrDefault(x => x.Id == id);

                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(System.Net.HttpStatusCode.NotFound, "News with ID = " + id + " not found");
                    }
                    else
                    {
                        dBContext.News.Remove(entity);
                        dBContext.SaveChanges();

                        return Request.CreateResponse(System.Net.HttpStatusCode.OK, entity);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
