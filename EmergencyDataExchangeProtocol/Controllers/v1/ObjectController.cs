﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmergencyDataExchangeProtocol.Auth;
using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.auth;
using EmergencyDataExchangeProtocol.Websocket;
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
    public partial class ObjectController : ControllerBase
    {
        IGenericDataStore db;
        AccessCheck access = new AccessCheck();
        ObjectChangeTracker changeTracker;

        /// <summary>
        /// Controller to Access the Documents
        /// </summary>
        /// <param name="db"></param>
        public ObjectController(IGenericDataStore db, ObjectChangeTracker ct)
        {
            this.db = db;
            this.changeTracker = ct;
        }

        private EndpointIdentity GetCurrentIdentity()
        {
            return Request.HttpContext.Items["Identity"] as EndpointIdentity;
        }

        private List<string> GetAccessablePaths(AccessLevelEnum acl, EmergencyObject obj, EndpointIdentity id)
        {
            return access.GetPathsByAccessLevel(acl, id.accessIdentity, obj.header.Access);
        }

        private List<string> GetAccessPaths(AccessLevelEnum requiredAccessLevel, EmergencyObject obj, EndpointIdentity id)
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

        /// <summary>
        /// Checks if the user can read the Subpath on the given Document
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks if the user can write to the document and the given Documentpath
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
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
        /// Checks if the user can change Permissions on the document
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CanGrant(EmergencyObject obj, EndpointIdentity id)
        {
            bool isOwner = (obj.header.createdBy == id.uid);

            if (isOwner)
                return true;

            /* Berechtigung über AccessLevel prüfen */
            if (access.CheckAccessForPath("*", id.accessIdentity, obj.header.Access, AccessLevelEnum.Grant) >= AccessLevelEnum.Grant)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Removes all Subpaths which are not allowed for this user to be viewed.
        /// </summary>
        /// <param name="allowedPaths"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private object RemoveUnallowedPaths(List<string> allowedPaths, object data)
        {
            var result = new JObject(data);



            return result;
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
