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
            settings.Server = new MongoServerAddress("edxp.bosmesh.org", 32768);
            settings.Credential = MongoCredential.CreateCredential("admin", "root", "myMongoDbPasswordForMeOnly");            
            settings.ConnectTimeout = new TimeSpan(0, 0, 3);
            settings.ServerSelectionTimeout = new TimeSpan(0, 0, 3);


            client = new MongoClient(settings);
            db = client.GetDatabase("edxp");
        }

        public IEnumerable<EndpointIdentity> GetAllEndpointsFromDatastore()
        {

            var obj = db.GetCollection<BsonDocument>("identities").Find(Builders<BsonDocument>.Filter.Empty).ToEnumerable();
            if (obj == null)
                yield break;

            foreach (var doc in obj)
            {
                yield return BsonSerializer.Deserialize<EndpointIdentity>(doc);
            }

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

        public DeleteObjectResult DeleteObjectInDatastore(Guid uid, string store)
        {
            var res = db.GetCollection<BsonDocument>(store).DeleteOne(t => t["_id"] == uid);

            DeleteObjectResult result = new DeleteObjectResult();
            result.deleted = (res.DeletedCount > 0);
            return result;

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

        public WriteResult UpdateIdentity(EndpointIdentity data)
        {
            try
            {
                var bson = data.ToBsonDocument();
                db.GetCollection<BsonDocument>("identities").ReplaceOne(x => x["_id"] == data.uid, bson);
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

        public bool DeleteIdentity(Guid uuid)
        {
            /* Alle Objekte deren Owner derjenige ist löschen */
            var numberOfDeletedObjects = db.GetCollection<EmergencyObject>("objects").DeleteMany(x => x.header.createdBy == uuid).DeletedCount;

            /* Endpunkt löschen */
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
