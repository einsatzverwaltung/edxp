using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.auth;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EmergencyDataExchangeProtocol.Datastore
{
    public class MongoDbInterface
    {
        MongoClient client;
        IMongoDatabase db;

        public MongoDbInterface()
        {

            var settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress("localhost", 27017);
            settings.ConnectTimeout = new TimeSpan(0, 0, 3);
            settings.ServerSelectionTimeout = new TimeSpan(0, 0, 3);

            client = new MongoClient(settings);
            db = client.GetDatabase("edxp");
        }

        public WriteResult CreateObjectInDatastore(EmergencyObject data, string store)
        {
            try
            {
                var bson = data.ToBsonDocument();
                db.GetCollection<BsonDocument>(store).InsertOne(bson);
                return WriteResult.OK;
            }
            catch (TimeoutException timeoutEx)
            {
                return WriteResult.ServerError;
            }
            catch (MongoWriteException we)
            {
                if (we.WriteError.Code == 11000)
                {
                    /* Duplicated */
                    return WriteResult.Duplicated;
                }
                else
                {
                    return WriteResult.ServerError;
                }
            }
        }

        public EmergencyObject GetObjectFromDatastore(Guid id, string store)
        {

            var obj = db.GetCollection<BsonDocument>(store).Find(x => x["_id"] == id).FirstOrDefault();
            if (obj == null)
                return null;
            return BsonSerializer.Deserialize<EmergencyObject>(obj);

        }

        public bool HasIdentities()
        {
            return db.GetCollection<BsonDocument>("identities").EstimatedDocumentCount() > 0;
        }

        public bool DeleteIdentity(Guid uuid)
        {
            return db.GetCollection<BsonDocument>("identities").DeleteOne(b => b["_id"] == uuid).IsAcknowledged;
        }

        public bool CreateIdentity(EndpointIdentity id)
        {
            try
            {
                var bson = id.ToBsonDocument();
                db.GetCollection<BsonDocument>("identities").InsertOne(bson);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public EndpointIdentity GetIdentityByApiKey(string apiKey)
        {
            var obj = db.GetCollection<BsonDocument>("identities").Find(x => x["apiKeys"].AsBsonArray.Contains(apiKey)).FirstOrDefault();
            if (obj == null)
                return null;
            return BsonSerializer.Deserialize<EndpointIdentity>(obj);
        }

        public EndpointIdentity GetIdentityById(Guid uid)
        {
            var obj = db.GetCollection<BsonDocument>("identities").Find(x => x["_id"] == uid).FirstOrDefault();
            if (obj == null)
                return null;
            return BsonSerializer.Deserialize<EndpointIdentity>(obj);
        }

        public enum WriteResult
        {
            OK = 0,
            Duplicated = 1,
            ServerError = 2
        }
    }
}
