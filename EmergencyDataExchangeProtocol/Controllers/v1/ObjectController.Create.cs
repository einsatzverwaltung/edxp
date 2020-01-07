using EmergencyDataExchangeProtocol.Datastore;
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
        /// Creates a new Object in the Datastore. The optional UID must be Unique.
        /// If an object with the same UID already exists then the Post Method fails.
        /// </summary>
        /// <param name="data">The data which should be stored</param>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(EmergencyObject))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        public ActionResult<EmergencyObject> CreateObject([FromBody] EmergencyObject data)
        {
            var identity = GetCurrentIdentity();

            if (data == null)
            {
                return new NoContentResult();
            }

            if (!data.uid.HasValue)
            {
                data.uid = Guid.NewGuid();
            }

            if (data.header == null)
            {
                data.header = new EmergencyObjectHeader();
            }
            data.header.created = DateTime.UtcNow;
            data.header.createdBy = identity.uid;
            data.header.lastUpdated = DateTime.UtcNow;
            data.header.documentVersion = 1;
            /* Minimum TTL is 120 minutes */
            if (data.header.timeToLive < 120 && data.header.timeToLive != 0)
            {
                data.header.timeToLive = 120;
            }
            /* Maximum TTL on this Server = 30 days */
            if (data.header.timeToLive > 60 * 24 * 30)
            {
                data.header.timeToLive = 60 * 24 * 30;
            }

            if (!EmergencyModelInformation.RegisteredDataTypes.ContainsKey(data.header.dataType))
            {
                var details = new ProblemDetails();
                details.Status = 400;
                details.Title = "Specified Data Type \"" + data.header.dataType + "\" is not known!";
                return new BadRequestObjectResult(details);
            }

            var dataType = EmergencyModelInformation.RegisteredDataTypes[data.header.dataType];

            /* Check TTL for datatype */
            if (dataType.MaximumTimeToLive != 0)
            {
                if (data.header.timeToLive > dataType.MaximumTimeToLive || data.header.timeToLive == 0)
                {
                    data.header.timeToLive = dataType.MaximumTimeToLive;
                }
            }


            var body = ((JObject)data.data).ToObject(dataType.Typ);

            var valid = ModelState.IsValid;

            TryValidateModel(body);

            if (ModelState.IsValid)
            {
                data.data = body;

                var res = db.CreateObjectInDatastore(data);

                if (res.writeResult == WriteResult.OK)
                {
                    changeTracker.ObjectChanged(data.uid.Value, Websocket.Message.MessageSentTrigger.Created, data, null);

                    return new CreatedResult("/object/" + data.uid.Value.ToString(), data);
                }
                else if (res.writeResult == WriteResult.Duplicated)
                {
                    return new ConflictResult();
                }
                else
                {
                    return new StatusCodeResult(500);
                }
            }
            else
            {
                var problems = new ValidationProblemDetails(ModelState);

                return BadRequest(problems);

            }

        }
    }
}
