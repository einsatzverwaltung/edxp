using EmergencyDataExchangeProtocol.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Controllers.v1
{
    public partial class ObjectController
    {

        /// <summary>
        /// Returns an partial Object from the Datastore based on the given SubPath Structure of the document. Identified by its unique ID.
        /// </summary>
        /// <param name="id">Unique ID of the Document</param>
        /// <param name="subpath">Path of the Document Field Structure where the document should be started from</param>
        /// <returns></returns>
        [HttpGet("{id:guid}/{**subpath}")]
        [ProducesResponseType(200, Type = typeof(object))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public ActionResult<object> GetObjectPart(Guid id, string subpath)
        {
            var res = db.GetObjectFromDatastore(id);

            if (res.data == null)
                return NotFound();

            var emergencyObject = res.data as EmergencyObject;

            if (!CanRead(emergencyObject, GetCurrentIdentity(), subpath))
                return Forbid();

            var tokens = subpath.Split('/');
            var path = string.Join("[0].", tokens);

            var dataJobject = JObject.FromObject(res.data.data);

            var partOfResult = dataJobject.SelectToken(path);

            if (res == null)
                return NotFound();
            return Ok(partOfResult);
        }

        /// <summary>
        /// Returns an Object from the Datastore. Identified by its unique ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(200, Type = typeof(EmergencyObject))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public ActionResult<EmergencyObject> GetObject(Guid id)
        {
            var res = db.GetObjectFromDatastore(id);
            var emergencyObject = res.data as EmergencyObject;

            if (emergencyObject == null)
                return NotFound();

            var accessablePaths = GetAccessPaths(AccessLevelEnum.Read, emergencyObject, GetCurrentIdentity());

            // Kein Zugriff auf diese Objekt möglich
            if (accessablePaths.Count == 0)
                return Forbid();

            if (accessablePaths.Contains("*"))
            {
                /* Lese-Zugriff auf gesamtes Objekt möglich => Wird direkt zurückgegeben */
                return Ok(emergencyObject);
            }
            else
            {
                /* Lese-Zugriff nur auf bestimmte Unterpfade möglich, daher reduzieren wir das Objekt, dass wir zurück geben */
                emergencyObject.data = RemoveUnallowedPaths(accessablePaths, emergencyObject.data);
                return Ok(emergencyObject);
            }
        }


        /// <summary>
        /// Returns an Object from the Datastore. Identified by its unique ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("modified/{since}")]
        [ProducesResponseType(200, Type = typeof(ICollection<EmergencyObject>))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public ActionResult<ICollection<EmergencyObject>> GetModifiedObjects(DateTime since)
        {

            var res = db.GetObjectsFromDatastoreSince(since);
            List<EmergencyObject> result = new List<EmergencyObject>();

            foreach(var emergencyObject in res)
            {

                var accessablePaths = GetAccessPaths(AccessLevelEnum.Read, emergencyObject, GetCurrentIdentity());

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

            return Ok(result);
        }
    }
}
