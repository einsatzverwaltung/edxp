using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmergencyDataExchangeProtocol.EmergencyObjects.einsatz;
using EmergencyDataExchangeProtocol.EmergencyObjects.einsatzmittel;
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

        static MongoDbInterface()
        {

            BsonClassMap.RegisterClassMap<Einsatz>();
            BsonClassMap.RegisterClassMap<Einsatzmittel>();
        }

        public MongoDbInterface()
        {

            var settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress("localhost", 27017);
            settings.ConnectTimeout = new TimeSpan(0, 0, 3);
            settings.ServerSelectionTimeout = new TimeSpan(0, 0, 3);


            client = new MongoClient(settings);
            db = client.GetDatabase("edxp");
        }




        public WriteResult UpdateObjectInDatastore(EmergencyObject data, string store)
        {
            try
            {
                var bson = data.ToBsonDocument();
                db.GetCollection<BsonDocument>(store).ReplaceOne(x => x["_id"] == data.uid, bson);
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
            return db.GetCollection<EndpointIdentity>("identities").EstimatedDocumentCount() > 0;
        }

        public bool DeleteIdentity(Guid uuid)
        {
            return db.GetCollection<EndpointIdentity>("identities").DeleteOne(b => b.uid == uuid).IsAcknowledged;
        }

        public bool CreateIdentity(EndpointIdentity id)
        {
            try
            {
                var bson = id;
                db.GetCollection<EndpointIdentity>("identities").InsertOne(bson);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public EndpointIdentity GetIdentityByApiKey(string apiKey)
        {            
            var obj = db.GetCollection<EndpointIdentity>("identities").Find(x => x.apiKeys.Any(t => t == apiKey)).FirstOrDefault();
            if (obj == null)
                return null;
            return obj;
        }

        public EndpointIdentity GetIdentityById(Guid uid)
        {
            var obj = db.GetCollection<EndpointIdentity>("identities").Find(x => x.uid == uid).FirstOrDefault();
            if (obj == null)
                return null;
            return obj;
        }


    }
}
