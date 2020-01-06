using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.auth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Service
{
    public class ObjectService
    {
        IGenericDataStore db;
        AccessCheck access = new AccessCheck();

        /// <summary>
        /// Controller to Access the Documents
        /// </summary>
        /// <param name="db"></param>
        public ObjectService(IGenericDataStore db)
        {
            this.db = db;

        }

        public ICollection<EmergencyObject> GetModifiedObjects(DateTime since, EndpointIdentity id)
        {
            var res = db.GetObjectsFromDatastoreSince(since);
            List<EmergencyObject> result = new List<EmergencyObject>();

            foreach (var emergencyObject in res)
            {

                var accessablePaths = GetAccessPaths(AccessLevelEnum.Read, emergencyObject, id);

                // Kein Zugriff auf diese Objekt möglich
                if (accessablePaths.Count > 0)
                {
                    if (accessablePaths.Contains("*"))
                    {
                        /* Lese-Zugriff auf gesamtes Objekt möglich => Wird direkt zurückgegeben */
                        result.Add(emergencyObject);
                    }
                    else
                    {
                        /* Lese-Zugriff nur auf bestimmte Unterpfade möglich, daher reduzieren wir das Objekt, dass wir zurück geben */
                        emergencyObject.data = RemoveUnallowedPaths(accessablePaths, emergencyObject.data);
                        result.Add(emergencyObject);
                    }
                }

            }

            return result;
        }


        /// <summary>
        /// Removes all Subpaths which are not allowed for this user to be viewed.
        /// </summary>
        /// <param name="allowedPaths"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private object RemoveUnallowedPaths(List<string> allowedPaths, object data)
        {
            var result = new JObject(data);



            return result;
        }


        private List<string> GetAccessPaths(AccessLevelEnum requiredAccessLevel, EmergencyObject obj, EndpointIdentity id)
        {
            List<string> res;
            bool isOwner = (obj.header.createdBy == id.uid);

            if (isOwner)
            {
                res = new List<string>() { "*" };
                return res;
            }

            res = access.GetPathsByAccessLevel(requiredAccessLevel, id.accessIdentity, obj.header.Access);

            return res;
        }
    }
}
