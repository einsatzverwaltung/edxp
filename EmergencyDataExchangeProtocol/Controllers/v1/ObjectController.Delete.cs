using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Websocket;
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
        /// Deletes an Object from Objectstore by its UID. This is only allowed by it's Owner
        /// or someone with Owner Permissions.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpDelete("{uid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult DeleteObject(Guid uid)
        {
            var result = db.GetObjectFromDatastore(uid);

            if (result.data == null)
                return NotFound();

            var identity = GetCurrentIdentity();

            List<string> access = GetAccessPaths(AccessLevelEnum.Owner, result.data, identity);

            if (access.Contains("*"))
            {
                db.DeleteObjectInDatastore(uid);
                changeTracker.ObjectChanged(uid, Websocket.Message.MessageSentTrigger.Deleted, result.data, null, null);
                return Ok();
            }
            else
            {
                return Forbid();
            }
        }
    }
}
