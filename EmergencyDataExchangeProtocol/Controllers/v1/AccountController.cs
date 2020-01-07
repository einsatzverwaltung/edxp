using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models.account;
using EmergencyDataExchangeProtocol.Models.auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyDataExchangeProtocol.Controllers.v1
{
    /// <summary>
    /// Exposes an Endpoint to manage Accounts on this Server
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "ExternalApi")]
    public class AccountController : ControllerBase
    {

        IGenericDataStore db;

        /// <summary>
        /// Controller to Access the Documents
        /// </summary>
        /// <param name="db"></param>
        public AccountController(IGenericDataStore db)
        {
            this.db = db;
        }


        private EndpointIdentity GetCurrentIdentity()
        {
            return Request.HttpContext.Items["Identity"] as EndpointIdentity;
        }

        /// <summary>
        /// Returns Information about the current logged in Account
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(MyAccount))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<MyAccount> GetAccountInfo()
        {
            var identity = GetCurrentIdentity();
            MyAccount res = new MyAccount();
            res.id = identity.uid;
            res.name = identity.name;
            res.accessIdentities = identity.accessIdentity;
            res.isServerAdmin = identity.isServerAdmin;
            res.contact = identity.contact;
            return res;
        }

        /// <summary>
        /// Returns a List of all available Endpoints on this EDXP Instance
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PublicAccountInfo>))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IEnumerable<PublicAccountInfo>> GetAccountList()
        {
            var enumerator = db.GetEndpointIdentities();

            var res = enumerator.Select(x => new PublicAccountInfo()
            {
                id = x.uid,
                accessKeys = x.accessIdentity,
                contact = x.contact,
                name = x.name
            });

            return res;
        }

        /// <summary>
        /// Holt Accountinformationen anhand einer ID aus der Datenbank
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "Get")]
        [ProducesResponseType(typeof(PublicAccountInfo), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult GetAccount(Guid id)
        {
            var identity = db.GetEndpointIdentityByGuid(id);
            if (identity == null)
                return NotFound();

            PublicAccountInfo res = new PublicAccountInfo();
            res.id = identity.uid;
            res.name = identity.name;
            res.accessKeys = identity.accessIdentity;
            res.contact = identity.contact;
            return Ok(res);
        }

        /// <summary>
        /// Creates a new Account
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        [ProducesResponseType(typeof(MyAccountWithApiKeys), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateNewAccount([FromBody] CreateAccountRequest value)
        {
            var identity = GetCurrentIdentity();

            if (!identity.isServerAdmin)
            {
                return Forbid();
            }

            var result = new MyAccountWithApiKeys();

            var endpoint = db.CreateNewEndpoint(value.name, value.isServerAdmin, value.accessIdentities, value.contact);

            result.id = endpoint.uid;
            result.name = endpoint.name;
            result.isServerAdmin = endpoint.isServerAdmin;
            result.accessIdentities = endpoint.accessIdentity;
            result.apiKeys = endpoint.apiKeys;
            result.contact = identity.contact;

            return Ok(result);
        }

        /// <summary>
        /// Aktualisieren der Accountinformationen
        /// </summary>
        /// <param name="data">Eigene Accountinfos</param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MyAccount), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] MyAccount data)
        {
            var identity = GetCurrentIdentity();

            if (!identity.isServerAdmin)
            {
                return Forbid();
            }

            var updateIdentity = db.GetEndpointIdentityByGuid(id);

            if (updateIdentity == null)
                return NotFound();

            updateIdentity.contact = data.contact;
            updateIdentity.name = data.name;
            updateIdentity.accessIdentity = data.accessIdentities;
            updateIdentity.isServerAdmin = data.isServerAdmin;

            db.UpdateEndpointInDatastore(updateIdentity);

            data.id = updateIdentity.uid;

            return Ok(data);
        }

        /// <summary>
        /// Aktualisieren der Accountinformationen
        /// </summary>
        /// <param name="data">Eigene Accountinfos</param>
        [HttpPut()]
        [ProducesResponseType(typeof(MyAccount), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateMyAccount([FromBody] MyAccount data)
        {
            var identity = GetCurrentIdentity();

            identity.contact = data.contact;
            identity.name = data.name;

            data.accessIdentities = identity.accessIdentity;
            data.isServerAdmin = identity.isServerAdmin;

            db.UpdateEndpointInDatastore(identity);

            data.id = identity.uid;

            return Ok(data);
        }

        /// <summary>
        /// Deletes an Account (And all objects with this account as owner)
        /// </summary>
        /// <param name="id">ID of the Account</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var identity = GetCurrentIdentity();

            if (!identity.isServerAdmin)
            {
                return Forbid();
            }

            db.DeleteEndpoint(id);

            return Ok();
        }

        /// <summary>
        /// Returns a List of all API Keys for the current authorized Account
        /// </summary>
        /// <returns></returns>
        [HttpGet("apiKeys")]
        [ProducesResponseType(200, Type = typeof(List<string>))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetApiKeyList()
        {
            var identity = GetCurrentIdentity();

            if (!identity.isServerAdmin)
            {
                return Forbid();
            }

            return Ok(identity.apiKeys);
        }
    }
}
