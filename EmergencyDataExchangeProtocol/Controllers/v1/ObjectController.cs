using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EmergencyDataExchangeProtocol.Controllers.v1
{
    /// <summary>
    /// Controller for Generic Object Modifications
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ObjectController : ControllerBase
    {
        IGenericDataStore db;

        Dictionary<EmergencyObjectDataTypes, Type> registeredDataTypes = new Dictionary<EmergencyObjectDataTypes, Type>()
        {
            [EmergencyObjectDataTypes.Einsatz] = typeof(EmergencyObjects.einsatz.Einsatz),
            [EmergencyObjectDataTypes.Einsatzmittel] = typeof(EmergencyObjects.einsatzmittel.Einsatzmittel)
        };

        public ObjectController(IGenericDataStore db)
        {
            this.db = db;
        }

        EndpointIdentity identity = new EndpointIdentity()
        {
            accessIdentity = new List<string>() { "fw.eu.de.he.da.mkk.mtl" },
            name = "Testidentität",
            uid = new Guid("73362a0a-9635-4de5-9c6d-23f0b5620e11")
        };

        /// <summary>
        /// Returns an Object from the Datastore. Identified by its unique ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = "ApiKeyPolicy")]
        public ActionResult<EmergencyObject> GetObject(Guid id)
        {

            var res = db.GetObjectFromDatastore(id, identity);
            if (res == null)
                return NotFound();
            return Ok(res);
        }

        /// <summary>
        /// Returns an Object from the Datastore. Identified by its unique ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}/{**subpath}")]
        public ActionResult<EmergencyObject> GetObjectPart(Guid id, string subpath)
        {
            var res = db.GetObjectFromDatastore(id, identity);
            
            var tokens = subpath.Split('/');
            var path = string.Join("[0].", tokens);

            var dataJobject = new JObject(res.data);

            var partOfResult = dataJobject.SelectToken(path);

            if (res == null)
                return NotFound();
            return Ok(partOfResult);
        }

        /// <summary>
        /// Creates a new Object in the Datastore. The optional UID must be Unique.
        /// If an object with the same UID already exists then the Post Method fails.
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public ActionResult<EmergencyObject> CreateObject([FromBody] EmergencyObject data)
        {
            AccessCheck ac = new AccessCheck();

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
            data.header.createdBy = identity.uid; // TODO
            if (data.header.timeToLive < 120 && data.header.timeToLive != 0)
            {
                data.header.timeToLive = 120;
            }
            if (data.header.timeToLive > 60 * 24 * 30)
            {
                data.header.timeToLive = 60 * 24 * 30;
            }

            if (!registeredDataTypes.ContainsKey(data.header.dataType))
            {
                return new BadRequestResult();
            }

            var dataType = registeredDataTypes[data.header.dataType];

            var body = ((JObject)data.data).ToObject(dataType);

            var valid = ModelState.IsValid;

            TryValidateModel(body);

            data.data = body;

            var res = db.CreateObjectInDatastore(data, identity);

            return res;
        }

        // PUT: api/Object/5
        [HttpPut("{id}")]
        public void UpdateObject(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void DeleteObject(int id)
        {
        }

        [HttpPatch("{uid}")]
        public void PatchObject(Guid uid, [FromBody]JsonPatchDocument jsonPatchDocument)
        {

            var res = db.GetObjectFromDatastore(uid, identity);

            jsonPatchDocument.ApplyTo(res.data);

            

        }
    }
}
