using EmergencyDataExchangeProtocol.Models.auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.account
{
    public class CreateAccountRequest
    {
        [Required, MinLength(5)]
        public string name { get; set; }
        [Required]
        public bool isServerAdmin { get; set; }
        [Required]
        public List<string> accessIdentities { get; set; }
        [Required]
        public ContactDetails contact { get; set; }
    }
}
