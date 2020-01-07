using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Websocket;
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
        /// Updates an entire Object. If the Document doesn't exist it will be created. Otherwise the document is updated.
        /// </summary>
        /// <param name="id">The ID of the object</param>
        /// <param name="data">The new data to replace the old data</param>
        [HttpPut("{uid:guid}")]
        [ProducesResponseType(200, Type = typeof(EmergencyObject))]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<object>> UpdateObject(Guid uid, [FromBody] EmergencyObject data)
        {
            var result = db.GetObjectFromDatastore(uid);
            var identity = GetCurrentIdentity();

            if (result.data == null)
                return NotFound();

            var res = result.data as EmergencyObject;

            if (!CanWrite(res, identity, null))
                return Forbid();

            var dataType = EmergencyModelInformation.RegisteredDataTypes[res.header.dataType].Typ;

            var body = ((JObject)data.data).ToObject(dataType);

            try
            {
                TryValidateModel(body);
            }
            catch (Exception ex)
            {
                var problems = new ValidationProblemDetails(ModelState);
                return BadRequest(problems);
            }

            res.data = body;

            res.header.documentVersion++;
            res.header.lastUpdated = DateTime.UtcNow;
            res.header.lastUpdatedBy = identity.uid;

            if (ModelState.IsValid)
            {
                db.UpdateObjectInDatastore(res);

                changeTracker.ObjectChanged(res.uid.Value, Websocket.Message.MessageSentTrigger.Updated, res, null);

                return Ok(res);
            }
            else
            {
                var problems = new ValidationProblemDetails(ModelState);
                return BadRequest(problems);
            }
        }

        /// <summary>
        /// Updates a part of an Object given by the subpath.
        /// </summary>
        /// <param name="uid">The ID of the object</param>
        /// <param name="subpath">The sub path under which the data should be updated</param>
        /// <param name="data">The new data to replace the old data</param>
        [HttpPut("{uid:guid}/{**subpath}")]
        [ProducesResponseType(200, Type = typeof(EmergencyObject))]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<object>> UpdatePartialObject(Guid uid, string subpath, [FromBody] JToken data)
        {
            var result = db.GetObjectFromDatastore(uid);
            var identity = GetCurrentIdentity();

            if (result.data == null)
                return NotFound();

            var res = result.data as EmergencyObject;

            if (!CanWrite(res, identity, subpath))
                return Forbid();

            var dataType = EmergencyModelInformation.RegisteredDataTypes[res.header.dataType].Typ;

            var tokens = subpath.Split('/');
            var path = string.Join("[0].", tokens);

            var body = JObject.FromObject(res.data);

            var selectedSubObject = body.SelectToken(path);

            if (selectedSubObject != null)
            {
                selectedSubObject.Replace(data);
            }
            else
            {
                body.Add(path, data);
            }

            var bodyTyped = body.ToObject(dataType);

            TryValidateModel(bodyTyped);

            res.data = bodyTyped;

            res.header.documentVersion++;
            res.header.lastUpdated = DateTime.UtcNow;
            res.header.lastUpdatedBy = identity.uid;

            if (ModelState.IsValid)
            {
                db.UpdateObjectInDatastore(res);
                changeTracker.ObjectChanged(res.uid.Value, Websocket.Message.MessageSentTrigger.Updated, res, data, path);
                return Ok(res);
            }
            else
            {
                var problems = new ValidationProblemDetails(ModelState);
                return BadRequest(problems);
            }
        }
    }
}
