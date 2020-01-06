using EmergencyDataExchangeProtocol.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Controllers.v1
{
    public partial class ObjectController
    {

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
