using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LinqToTwitter;
using System.Configuration;
using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace SentimentAnalysis.Controllers
{
    public class Tweet
    {
        private string _userName = "No UserName";

        [BsonId]
        public string ID
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public string City
        {
            get;
            set;
        }

        public string State
        {
            get;
            set;
        }

        public override string ToString()
        {
            return String.Format("User : {0}, Tweet : {1}, City {2}, State : {3}", UserName, Text, City, State);
        }
    
    }

    public class AggregateCount
    {

        public Int64 Count
        {
            get;
            set;
        }
    
    }

    public class StateTweetCount
    {
        public string State
        {
            get;
            set;
        }

        public Int64 Count
        {
            get;
            set;
        }
        private string id;

        public string _id
        {
            get {
                return id;
            }
            set {
                id = value;
                State = value;
            }
        
        }
        private AggregateCount _value;
        public AggregateCount value
        {
            get {
                return _value;
            }
            set {
                _value = value;
                Count = value.Count;
            }
    
        }
    }

    public class TwitterWrapper
    {
        private SingleUserAuthorizer _auth;
        private List<string> _searchStrings;
        private const int TwitterRateLimitPerHour = 150; //number of requests and API can make to twitter per hour. PreDefined by Twiiter Inc.
        private static List<string> cityStates = new List<string>();

        public TwitterWrapper()
        {
            _auth = new SingleUserAuthorizer
            {
                Credentials = new InMemoryCredentials
                {// This is authorizing my app to interact with Twitter.
                    ConsumerKey = ConfigurationManager.AppSettings["TwitterConsumerKey"],
                    ConsumerSecret = ConfigurationManager.AppSettings["TwitterConsumerSecret"],
                    OAuthToken = ConfigurationManager.AppSettings["TwitterAccessToken"],
                    AccessToken = ConfigurationManager.AppSettings["TwitterAccessTokenSecret"]
                }
            };

            if (!cityStates.Any())
            {
                LoadCities();
            }

            _searchStrings = new List<string>();
            _searchStrings.Add("Democrat OR Republican");
            _searchStrings.Add("Snow");
        }

        public void GatherTweets(int tweetCount = 500)
        {
            List<Tweet> allTweets = new List<Tweet>();
            Random random = new Random();

            try
            {
                var twitterCtx = new LinqToTwitter.TwitterContext(_auth);
                if (twitterCtx != null)
                {
                    int count = 0;
                    int searchRequests = 0;
                    if (twitterCtx.Status != null)
                    {
                        while (count < tweetCount) //run untill we have atleast x tweets or we have hit half of the rate limit
                        {
                            List<Search> searchResponses =
                            (from t in twitterCtx.Search
                             where
                             t.Type == SearchType.Search && t.Query == "Democrat OR Republican OR Snow OR Christmas" && t.IncludeEntities == true
                             select t).ToList();
                            searchRequests++;
                            foreach (Search searchResponse in searchResponses)
                            {
                                if (searchResponse != null && searchResponse.Statuses != null)
                                {
                                    foreach (Status status in searchResponse.Statuses)
                                    {
                                        //if we do not have the location of the user or its not in the correct format, lets assing a random city,state since this project is just a prototype.
                                        if (String.IsNullOrWhiteSpace(status.User.Location) || !status.User.Location.Contains(","))
                                        {
                                            int index = random.Next(cityStates.Count);
                                            status.User.Location = status.User.Location = cityStates.ElementAt(index);
                                        }
                                        if (String.IsNullOrWhiteSpace(status.StatusID)) //we do not want tweets with empty IDs
                                            continue;

                                        string[] cityAndState = status.User.Location.Split(new[] { ',' });
                                        if (cityAndState.Length != 2) //only if the location is in the format [City, State]
                                            continue;
                                        if (allTweets.Where(x => x.ID == status.ID).Any()) //if we already have a tweet with this ID then continue;
                                            continue;

                                        Tweet tweet = new Tweet();
                                        tweet.ID = status.StatusID;
                                        tweet.Text = status.Text;
                                        tweet.UserName = status.User.Name;
                                        tweet.City = cityAndState.First().Trim();
                                        tweet.State = cityAndState.Skip(1).First().Trim();
                                        allTweets.Add(tweet);
                                        count++; //since we added another tweet to our list, we are incrementing the count.
                                    }
                                }
                            }
                        }
                    }

                    Debug.WriteLine(String.Format("Twitter Requests Made: {0}", searchRequests));
                }
            }
            catch (Exception e)
            {
                //eating the exception.
            }

            //save all the tweets collected
            try
            {
                DataManager dataManager = new DataManager();
                if (allTweets.Any())
                    dataManager.AddTweets(allTweets);
            }
            catch(Exception e)
            {
            }
        }

        public void LoadCities()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SentimentAnalysis.Resources.data.txt";
            List<string> lines = new List<string>();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (reader.Peek() >= 0)
                    lines.Add(reader.ReadLine());
            }
            foreach (string line in lines)
            {
                string[] tokens = line.Split(new char[] { ',' });
                string city = tokens[1];
                if (IsNumeric(city))
                    continue;
                string state = tokens[3];
                cityStates.Add(String.Format("{0}, {1}", city, state));
            }
        }

        private static bool IsNumeric(string s)
        {
            double Result;
            return double.TryParse(s, out Result);
        }        
    }
}