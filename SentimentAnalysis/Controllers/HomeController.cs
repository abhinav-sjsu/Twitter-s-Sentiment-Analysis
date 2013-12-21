using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SentimentAnalysis.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to SocialAnalyzer. As an initial IaaS(Intelligence as a Service), we present Twitter Sentiment Analysis.";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "We intend to mine the twitter data and derive intelligence through sundry analysis. Currently, we support sentiment analysis and state ranks by number of tweets.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Talk to me now.";

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ViewStoredTweets()
        {
            TwitterWrapper twitterWrapper = new TwitterWrapper();
            DataManager dataManager = new DataManager();
            IList<Tweet> tweets = dataManager.ReadAllTweets();
            return View(tweets);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GatherMoreTweets()
        {
            TwitterWrapper twitterWrapper = new TwitterWrapper();
            twitterWrapper.GatherTweets();
            return RedirectToAction("ViewStoredTweets");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RankStatesByMood()
        {
            TweetAnalyzer tweetAnalyzer = new TweetAnalyzer();
            List<StateMood> stateMoods = tweetAnalyzer.Analyze();
            return View(stateMoods);
        }

        public ActionResult DeleteTweet(string id)
        {
            DataManager dataManager = new DataManager();
            dataManager.DeleteTweet(id.ToString());
            return RedirectToAction("ViewStoredTweets");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ChirpyStates()
        {
            DataManager dataManager = new DataManager();
            var result = dataManager.GetTweetsByState();
            return View(result);
        }
    }
}
