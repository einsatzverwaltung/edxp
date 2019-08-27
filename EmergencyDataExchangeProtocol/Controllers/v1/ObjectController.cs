using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmergencyDataExchangeProtocol.Auth;
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
    [Authorize(Policy = "ExternalApi")]
    public class ObjectController : ControllerBase
    {
        IGenericDataStore db;
        AccessCheck access = new AccessCheck();

        Dictionary<EmergencyObjectDataTypes, Type> registeredDataTypes = new Dictionary<EmergencyObjectDataTypes, Type>()
        {
            [EmergencyObjectDataTypes.Einsatz] = typeof(EmergencyObjects.einsatz.Einsatz),
            [EmergencyObjectDataTypes.Einsatzmittel] = typeof(EmergencyObjects.einsatzmittel.Einsatzmittel)
        };

        public ObjectController(IGenericDataStore db)
        {
            this.db = db;
            
        }

        private EndpointIdentity GetCurrentIdentity()
        {
            return Request.HttpContext.Items["Identity"] as EndpointIdentity;                     
        }

        private List<string> GetAccessablePaths(AccessLevelEnum acl, EmergencyObject obj, EndpointIdentity id)
        {
            return access.GetPathsByAccessLevel(acl, id.accessIdentity, obj.header.Access);
        }

        private List<string> GetReadPaths(AccessLevelEnum requiredAccessLevel, EmergencyObject obj, EndpointIdentity id)
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

        private bool CanRead(EmergencyObject obj, EndpointIdentity id, string subPath)
        {
            bool isOwner = (obj.header.createdBy == id.uid);

            /* Nutzer ist Besitzer des Objekts */
            if (isOwner)
                return true;

            /* Berechtigung über AccessLevel prüfen */
            if (access.CheckAccessForPath(subPath, id.accessIdentity, obj.header.Access, AccessLevelEnum.Read) >= AccessLevelEnum.Read)
            {
                return true;
            }

            return false;
        }

        private bool CanWrite(EmergencyObject obj, EndpointIdentity id, string subPath)
        {
            bool isOwner = (obj.header.createdBy == id.uid);

            if (isOwner)
                return true;

            /* Berechtigung über AccessLevel prüfen */
            if (access.CheckAccessForPath(subPath, id.accessIdentity, obj.header.Access, AccessLevelEnum.Write) >= AccessLevelEnum.Write)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns an Object from the Datastore. Identified by its unique ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public ActionResult<EmergencyObject> GetObject(Guid id)
        {

            var res = db.GetObjectFromDatastore(id);
            var emergencyObject = res.data as EmergencyObject;

            if (emergencyObject == null)
                return NotFound();

            var accessablePaths = GetReadPaths(AccessLevelEnum.Read, emergencyObject, GetCurrentIdentity());

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

        private object RemoveUnallowedPaths(List<string> allowedPaths, object data)
        {
            var result = new JObject(data);



            return result;
        }

        /// <summary>
        /// Returns an Object from the Datastore. Identified by its unique ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}/{**subpath}")]
        public ActionResult<object> GetObjectPart(Guid id, string subpath)
        {
            var res = db.GetObjectFromDatastore(id);
            var emergencyObject = res.data as EmergencyObject;

            if (!CanRead(emergencyObject, GetCurrentIdentity(), subpath))
                return Forbid();

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
        [ProducesResponseType(201, Type = typeof(EmergencyObject))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        public ActionResult<EmergencyObject> CreateObject([FromBody] EmergencyObject data)
        {
            AccessCheck ac = new AccessCheck();
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
                var details = new ProblemDetails();
                details.Status = 400;
                details.Title = "Specified Data Type \"" + data.header.dataType + "\" is not known!";
                return new BadRequestObjectResult(details);
            }

            var dataType = registeredDataTypes[data.header.dataType];

            var body = ((JObject)data.data).ToObject(dataType);

            var valid = ModelState.IsValid;

            TryValidateModel(body);

            if (ModelState.IsValid)
            {
                data.data = body;

                var res = db.CreateObjectInDatastore(data);

                if (res.writeResult == WriteResult.OK)
                {
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

        [HttpPatch("jsonpatch/{uid}")]
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

        [HttpPatch("increment/{uid}")]

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

        private void UpdateFieldsToObject(JObject update, object old)
        {
            var typ = old.GetType();

            foreach (var t in update)
            {
                var prop = typ.GetProperty(t.Key);
                if (prop != null)
                {
                    if (t.Value is JObject)
                    {
                        var memberInstance = prop.GetValue(old);
                        UpdateFieldsToObject((JObject)t.Value, memberInstance);
                    }
                    else
                    {
                        prop.SetValue(old, ((JValue)t.Value).Value);
                    }
                }
            }
        }
    }
}
