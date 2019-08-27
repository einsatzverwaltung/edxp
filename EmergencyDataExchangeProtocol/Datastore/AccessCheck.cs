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

        public List<string> GetPathsByAccessLevel(AccessLevelEnum requiredAccessLevel, List<string> identifiers, List<EmergenyObjectAccessContainer> accessList)
        {
            List<string> result = new List<string>();

            if (identifiers == null || accessList == null)
                return result;

            foreach (var identifier in identifiers)
            {
                foreach (var accessEntry in accessList)
                {
                    if (IdentifierMatches(identifier, accessEntry.idPattern))
                    {
                        /* Identifier passt, Rechte übernehmen */
                        foreach (var pathLevel in accessEntry.acl)
                        {
                            if (pathLevel.level >= requiredAccessLevel)
                            {
                                result.Add(pathLevel.path);
                            }

                            if (pathLevel.path == "*")
                            {
                                return result;
                            }
                        }

                    }
                }
            }

            return result;
        }

        public AccessLevelEnum CheckAccessForPath(string documentPath, List<string> identifiers, List<EmergenyObjectAccessContainer> accessList, AccessLevelEnum requiredAccessLevel = AccessLevelEnum.Grant)
        {
            AccessLevelEnum level = AccessLevelEnum.None;

            foreach (var identifier in identifiers)
            {
                foreach (var accessEntry in accessList)
                {
                    if (IdentifierMatches(identifier, accessEntry.idPattern))
                    {
                        /* Identifier passt, Rechte übernehmen */
                        foreach(var pathLevel in accessEntry.acl)
                        {
                            if(pathLevel.path == "*" || documentPath.StartsWith(pathLevel.path))
                            {
                                if(pathLevel.level > level)
                                {
                                    level = pathLevel.level;
                                    if(level >= requiredAccessLevel)
                                    {
                                        return level;
                                    }
                                }
                            }
                        }

                    }
                }
            }

            return level;
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
