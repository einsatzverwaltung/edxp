using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.auth
{
    public class ContactDetails
    {
        [Required]
        public string contactName { get; set; }
        [Required]
        public string contactMail { get; set; }
    }
}
