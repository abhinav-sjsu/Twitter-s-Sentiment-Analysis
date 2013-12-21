using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SentimentAnalysis.Controllers
{
    /// <summary>
    /// This is just a simple Wrapper to hide the details of MongoDB connection stuff. Nothing fancy.
    /// </summary>
    public sealed class MongoWrapper
    {
        //Name of the database the user will be connecting to. E.g. you can make each Client Mobile App to create a new database for itself. This makes it easy to do house-keeping.
        private string _databaseName;

        //Server Name can be changed by the admin in your company. e.g. you can specify the IPAddress of the machine where you will install mongoDB. 
        //mongoDB and your web-service do not necessarily need to be on the same machine. You can keep localhost if you want. 
        //Localhost means that your web-service and mongoDB are on the same machine. 
        private string _serverName = "localhost";

        private string _connectionString;

        private MongoClient _mongoClient;
        private MongoServer _mongoServer;

        /// <summary>
        /// Initialize the MongoWrapper variable and properties.
        /// </summary>
        /// <param name="databaseName">Name of the database to connect to.</param>
        public void Initialize(string databaseName)
        {
            _databaseName = databaseName;
            _connectionString = String.Format("mongodb://{0}", _serverName);  //e.g.  "mongodb://localhost"
            _mongoClient = new MongoClient(_connectionString);
            _mongoServer = _mongoClient.GetServer();
        }

        /// <summary>
        /// Get the database that the user wants to work with. It will also create the database if it already does not exist.
        /// </summary>
        /// <returns></returns>
        public MongoDatabase GetDatabase()
        {
            MongoDatabase mongoDB = _mongoServer.GetDatabase(_databaseName);
            return mongoDB;
        }
    }
}