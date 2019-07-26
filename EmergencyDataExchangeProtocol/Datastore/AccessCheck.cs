using EmergencyDataExchangeProtocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Datastore
{
    /// <summary>
    /// This class checks the valid access level for a given Documentbranch checked against the
    /// location-access-identifier
    /// </summary>
    public class AccessCheck
    {
        public AccessCheck()
        {
           
        }

        public AccessLevelEnum CheckAccessForPath(string documentPath, List<string> identifiers, EmergencyObjectAccess accessList)
        {

            return AccessLevelEnum.None;
        }

        /// <summary>
        /// Prüft ob ein vorgebener Identifikationsstring von einem AccessSelector-Pattern getroffen wird.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accessSelector"></param>
        /// <returns></returns>
        public bool IdentifierMatches(string id, string accessSelector)
        {
            string accS = Regex.Escape(accessSelector);

            // Platzhalter ? steht für alle Zeichen außer Punkt
            accS = accS.Replace(@"\?", @"[^\.]*");
            // An das Ende muss ein Platzhalter angehängt werden, der alle Zeichen Matcht
            accS += "(.*)";

            var match = Regex.Match(id, accS, RegexOptions.IgnoreCase);
            return match.Success;
        }
    }
}
