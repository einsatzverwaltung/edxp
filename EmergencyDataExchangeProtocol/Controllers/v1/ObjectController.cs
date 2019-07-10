using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EmergencyDataExchangeProtocol.Controllers.v1
{
    /// <summary>
    /// Controller for Generic Object Modifications
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectController : ControllerBase
    {
        IGenericDataStore db;

        public ObjectController(IGenericDataStore db)
        {
            this.db = db;
        }

        /// <summary>
        /// Returns an Object from the Datastore. Identified by its unique ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "Get")]
        public ActionResult<EmergencyObject> Get(Guid id)
        {
            var res = db.GetObjectFromDatastore(id, new Models.auth.EndpointIdentity());
            if (res == null)
                return NotFound();
            return Ok(res);
        }

        /// <summary>
        /// Creates a new Object in the Datastore. The optional UID must be Unique.
        /// If an object with the same UID already exists then the Post Method fails.
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public ActionResult<EmergencyObject> Post([FromBody] EmergencyObject value)
        {
            EndpointIdentity id = new EndpointIdentity()
            {
                accessIdentity = new List<string>() { "fw.eu.de.he.da.mkk.mtl" },
                name = "Testidentität",
                uid = Guid.NewGuid()
            };
            var res = db.CreateObjectInDatastore(value, id);
            return res;
        }

        // PUT: api/Object/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpPatch("{id}")]
        public void Patch(Guid uid)
        {

        }
    }
}
