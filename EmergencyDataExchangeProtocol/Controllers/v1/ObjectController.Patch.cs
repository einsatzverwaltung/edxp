using EmergencyDataExchangeProtocol.Models;
using Microsoft.AspNetCore.JsonPatch;
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
        /// Modifies Object by applying Json Patch Document to existing Object
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="jsonPatchDocument"></param>
        /// <returns></returns>
        [HttpPatch("jsonpatch/{uid}")]
        [ProducesResponseType(200, Type = typeof(EmergencyObject))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        public IActionResult PatchObjectByJsonPatch(Guid uid, [FromBody]JsonPatchDocument jsonPatchDocument)
        {
            var result = db.GetObjectFromDatastore(uid);
            var identity = GetCurrentIdentity();

            if (result.data == null)
                return NotFound();

            var res = result.data as EmergencyObject;

            if (!CanWrite(res, identity, null))
                return Forbid();

            var writePaths = GetAccessablePaths(AccessLevelEnum.Write, res, identity);



            jsonPatchDocument.ApplyTo(res.data);

            TryValidateModel(res.data);

            res.header.documentVersion++;
            res.header.lastUpdated = DateTime.UtcNow;
            res.header.lastUpdatedBy = identity.uid;

            if (ModelState.IsValid)
            {
                db.UpdateObjectInDatastore(res);
                return Ok(res);
            }
            else
            {
                var problems = new ValidationProblemDetails(ModelState);
                return BadRequest(problems);
            }
        }

        /// <summary>
        /// Patch Documents by applying only set fields to existing Object in Datastore
        /// </summary>
        /// <param name="uid">ID of the Object which should be patched</param>
        /// <param name="jsonUpdateDocument">The JSON Document containing only these fields and attributes which should be updated in datastore</param>
        /// <returns>Returns the updated document</returns>
        [HttpPatch("partial/{uid}")]
        [ProducesResponseType(200, Type = typeof(EmergencyObject))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        public IActionResult PatchObjectByProperties(Guid uid, [FromBody] JObject jsonUpdateDocument)
        {
            var result = db.GetObjectFromDatastore(uid);
            var identity = GetCurrentIdentity();

            if (result.data == null)
                return NotFound();

            var res = result.data as EmergencyObject;

            // TODO: Subpath

            if (!CanWrite(res, identity, null))
                return Forbid();

            UpdateFieldsToObject(jsonUpdateDocument, res.data);

            TryValidateModel(res.data);

            res.header.documentVersion++;
            res.header.lastUpdated = DateTime.UtcNow;
            res.header.lastUpdatedBy = identity.uid;

            if (ModelState.IsValid)
            {
                db.UpdateObjectInDatastore(res);
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
