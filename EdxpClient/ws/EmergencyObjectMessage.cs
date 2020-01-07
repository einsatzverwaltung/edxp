using EdxpClient.edxp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace EdxpClient.ws
{
    public class EmergencyObjectMessage
    {
        /// <summary>
        /// Trigger Reason for this message
        /// </summary>
        [JsonProperty("@messageTrigger")]
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageSentTrigger messageTrigger { get; set; }

        public bool isParted { get; set; }
        public string partPath { get; set; }

        [JsonProperty("@uid")]
        public Guid? uid { get; set; }

        [JsonProperty("@header")]
        public EmergencyObjectHeader header { get; set; }

        public object data { get; set; }



    }

    /// <summary>
    /// Possible Reasons why the message was sent to the client
    /// </summary>
    public enum MessageSentTrigger
    {
        /// <summary>
        /// Object was created by an identity
        /// </summary>
        Created = 0,
        /// <summary>
        /// Object has been updated or patched by an identity
        /// </summary>
        Updated = 1,
        /// <summary>
        /// Object was deleted by its owner or someone with owner permissions
        /// </summary>
        Deleted = 2,
        /// <summary>
        /// Object was modified since the given timestamp so this message was sent due to initialization request after initial connect
        /// </summary>
        Init = 3
    }
}
