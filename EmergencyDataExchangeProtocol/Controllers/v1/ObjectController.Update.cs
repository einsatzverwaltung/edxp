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
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<object>> UpdateObject(int id, [FromBody] EmergencyObject data)
        {
            // TODO
            return null;
        }
    }
}
