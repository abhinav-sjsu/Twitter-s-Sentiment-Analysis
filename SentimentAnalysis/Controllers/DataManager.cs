using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace SentimentAnalysis.Controllers
{
    public class DataManager
    {
        private const string DatabaseName = "SentimentAnalysis";
        private const string rawTwitterDataCollection = "Tweets";

        /// <summary>
        /// Save all tweets to the database.
        /// </summary>
        /// <param name="tweets"></param>
        public void AddTweets(List<Tweet> tweets)
        {
            MongoWrapper mongoWrapper = new MongoWrapper();
            mongoWrapper.Initialize(DatabaseName);
            MongoDatabase database = mongoWrapper.GetDatabase();
            MongoCollection<Tweet> tweetCollection = database.GetCollection<Tweet>(rawTwitterDataCollection);
            tweetCollection.InsertBatch(tweets);
        }

        /// <summary>
        /// Read all the tweets from the database.
        /// </summary>
        /// <returns>List of all tweets from the database.</returns>
        public IList<Tweet> ReadAllTweets()
        {
            MongoWrapper mongoWrapper = new MongoWrapper();
            mongoWrapper.Initialize(DatabaseName);
            MongoDatabase database = mongoWrapper.GetDatabase();
            MongoCollection<Tweet> tweetCollection = database.GetCollection<Tweet>(rawTwitterDataCollection);
            List<Tweet> tweets = new List<Tweet>();
            foreach (var item in tweetCollection.FindAllAs<Tweet>())
            {
                Tweet tweet = new Tweet();
                tweet.ID = item.ID;
                tweet.UserName = item.UserName;
                tweet.Text = item.Text;
                tweet.City = item.City;
                tweet.State = item.State;
                tweets.Add(tweet);
            }
            return tweets;
        }

        /// <summary>
        /// Removes a tweet from the database.
        /// </summary>
        /// <param name="ID"></param>
        public void DeleteTweet(string ID)
        {
            MongoWrapper mongoWrapper = new MongoWrapper();
            mongoWrapper.Initialize(DatabaseName);
            MongoDatabase database = mongoWrapper.GetDatabase();
            MongoCollection<Tweet> tweetCollection = database.GetCollection<Tweet>(rawTwitterDataCollection);
            tweetCollection.Remove(Query.EQ("_id", ID), RemoveFlags.Single);
        }

        /// <summary>
        /// Return a list of StateTweetCount objects that lists each state with the number of tweets from that state.
        /// </summary>
        /// <returns>List of StateTweetCount objects sorted by count descending.</returns>
        public List<StateTweetCount> GetTweetsByState()
        {
            //throw new NotImplementedException();
            string map = @"
                function() {
                    var tweet = this;
                    emit(tweet.State,{Count:1});
                }";

            string reduce = @"        
                function(key, values) {
                    var result = {Count: 0};
                    values.forEach(function(value){               
                        result.Count += value.Count;
                    });

                    return result;
                }";

            MongoWrapper mongoWrapper = new MongoWrapper();
            mongoWrapper.Initialize(DatabaseName);
            MongoDatabase database = mongoWrapper.GetDatabase();
            MongoCollection<Tweet> tweetCollection = database.GetCollection<Tweet>(rawTwitterDataCollection);
            var options = new MapReduceOptionsBuilder();
            options.SetOutput(MapReduceOutput.Inline);
            var results = tweetCollection.MapReduce(map, reduce, options);
           List<StateTweetCount> list = new List<StateTweetCount>();
            foreach (var result in results.GetInlineResultsAs<StateTweetCount>())
            {
                list.Add(result);
            }
            return list.OrderByDescending(x => x.Count).ToList();
        }
    }
}