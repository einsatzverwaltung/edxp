using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Datastore
{
    public class GenericDataStore : IGenericDataStore
    {
        MongoDbInterface db;
        AccessCheck acl = new AccessCheck();
        ILogger logger;



        public GenericDataStore(ILoggerFactory log)
        {
            logger = log.CreateLogger<GenericDataStore>();
            db = new MongoDbInterface();
        }

        public void InitIdentity()
        {
            if (!db.HasIdentities())
            {
                /* Neue Identity anlegen */
                var newAdmin = new EndpointIdentity()
                {
                    accessIdentity = new List<string>() { "eu" },
                    apiKeys = new List<string>(),
                    name = "Default Admin Identity",
                    uid = Guid.NewGuid()
                };

                var key = new byte[48];
                using (var generator = RandomNumberGenerator.Create())
                    generator.GetBytes(key);
                string apiKey = Convert.ToBase64String(key);

                newAdmin.apiKeys.Add(apiKey);

                logger.LogInformation("There is no Identity - Created Admin with API Key " + apiKey);

                db.CreateIdentity(newAdmin);
            }
        }


        public GetObjectResult GetObjectFromDatastore(Guid uid)
        {
            var emergencyObject = db.GetObjectFromDatastore(uid, "objects");

            return new GetObjectResult()
            {
                data = emergencyObject
            };
        }

        public CreateObjectResult CreateObjectInDatastore(EmergencyObject data)
        {
            CreateObjectResult result = new CreateObjectResult();

            result.writeResult = db.CreateObjectInDatastore(data, "objects");
            result.createdObject = data;

            return result;

        }

        public EndpointIdentity GetEndpointIdentityByApiKey(string apiKey)
        {
            return db.GetIdentityByApiKey(apiKey);
        }

        public ActionResult<EmergencyObject> UpdateObjectInDatastore(EmergencyObject data)
        {

            var res = db.UpdateObjectInDatastore(data, "objects");
            if (res == WriteResult.OK)
            {
                return new CreatedResult("/object/" + data.uid.Value.ToString(), data);
            }
            else if (res == WriteResult.Duplicated)
            {
                return new ConflictResult();
            }
            else
            {
                return new StatusCodeResult(500);
            }
        }
    }
}
