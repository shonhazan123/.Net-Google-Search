using GoogleSearch.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace GoogleSearch.Controllers
{
    public class HomeController : Controller
    {
        private GoogleSearchEntities db = new GoogleSearchEntities();

        public ActionResult Index()
        {
            return View(new List<SearchResult>());
        }

        public ActionResult Search(string searchText)
        { 
            string searchUrl = "http://www.google.com/search?q=" + searchText;

            // Vars to store inside our DB

            var searchSummery = db.SearchSummeries.Create();
            searchSummery.SearchTerm = searchText;
            searchSummery.SearchTime = DateTime.UtcNow;
            searchSummery.ResultCount = 5;

            //start timer

            var timer = new Stopwatch();
            timer.Start();

            //initialize web client

            WebClient webClient = new WebClient();

            var result = new HtmlWeb().Load(searchUrl);

            var nodes = result.DocumentNode.SelectNodes("//html//body//div[@class='g']");

            //searching fineshed , stoping time

            timer.Stop();
            searchSummery.Duration = timer.Elapsed;
            ViewBag.Duration = timer.Elapsed.Milliseconds;

            // inserting each block of search in to a list 
            var searchItems = new List<SearchResult>();

            foreach (var node in nodes.Take(5)) //take 1st 5 results
            {
                var url = node.Descendants("a").FirstOrDefault().Attributes["href"].Value;
                var title = node.Descendants("a").FirstOrDefault().Descendants("div").FirstOrDefault().InnerText;
                var decription = node.Descendants("div").ElementAtOrDefault(6) != null ? node.Descendants("div").ElementAtOrDefault(6).InnerText : node.Descendants("div").ElementAtOrDefault(4).InnerText;
                var searchItem = new SearchResult()
                {
                    Title = title,
                    Url = url,
                    Description = decription,
                };

                searchItems.Add(searchItem);
            }

            //save the result summery to database
            
            db.SearchSummeries.Add(searchSummery);
            db.SaveChanges();

            //load search result in partial view

            return PartialView("_searchResultPartial", searchItems.ToArray());
        }

        public ActionResult SearchHistory()
        {
            //get search history from database
            var searchSummery = db.SearchSummeries.OrderByDescending(s => s.SearchTime).ToList();
            return View(searchSummery);
        }
    }
}