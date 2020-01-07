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

            res.data = data.data;
            
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
