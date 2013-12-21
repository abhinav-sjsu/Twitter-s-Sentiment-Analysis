using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace SentimentAnalysis.Controllers
{
    public class TweetAnalyzer
    {
        public TweetAnalyzer()
        {
            AffFileReader.ReadFile();
        }

        public List<StateMood> Analyze()
        {    
            DataManager dataManager = new DataManager();
            Dictionary<string, Int32> StateSentimentMap = new Dictionary<string, int>();
            List<StateTweetCount> stateTweetCounts = dataManager.GetTweetsByState();
           
            IList<Tweet> tweets = dataManager.ReadAllTweets();

            foreach (Tweet tweet in tweets)
            {
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex(@"[ ]{2,}", options);
                string tempTweet = regex.Replace(tweet.Text, @" ").ToLower();
                string[] tweetTokens = tempTweet.Split(new char[] { ' ' });
                int tweetSentiment = 0;
                foreach (string tweetToken in tweetTokens)
                {
                    tweetSentiment += AffFileReader.GetWordDegree(tweetToken);
                }

                if (StateSentimentMap.ContainsKey(tweet.State))
                {
                    StateSentimentMap[tweet.State] += tweetSentiment;
                }
                else
                {
                    StateSentimentMap.Add(tweet.State, tweetSentiment);
                }
            }

            //sort the dictionary by sentiment score so that state with most +ve sentiment is on top.
            List<StateMood> stateMoods = new List<StateMood>();
            foreach (KeyValuePair<string, Int32> pair in StateSentimentMap)
            {
                StateMood stateMood = new StateMood();
                Int64 Count = stateTweetCounts.Where(x => x.State == pair.Key).First().Count;
                stateMood.State = pair.Key;
                stateMood.MoodLevel = (Int32)Math.Ceiling(Convert.ToDouble(pair.Value/(Int32)Count));
                stateMoods.Add(stateMood);
            }

            return stateMoods.OrderByDescending(x => x.MoodLevel).ToList();
        }
    }
}