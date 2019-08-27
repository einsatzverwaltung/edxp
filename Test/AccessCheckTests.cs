using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace Test
{
    public class AccessCheckTests
    {
        [Fact]
        public void Identifiers()
        {
            AccessCheck ac = new AccessCheck();
            // Gilt für alle Feuerwehren Deutschlands - Matcht Feuerwehr Main-Kinzig
            Assert.True(ac.IdentifierMatches("fw.de.he.da.mkk", "fw.de"));
            // Gilt für Feuerwehr GElnhausen, Feuerwehr Maintal darf nicht matchen
            Assert.False(ac.IdentifierMatches("fw.de.he.da.mkk.mtl", "fw.de.he.da.mkk.gn"));
            // Gilt für alle Organisationen im MKK
            Assert.True(ac.IdentifierMatches("fw.de.he.da.mkk.mtl", "?.de.he.da.mkk"));
        }

        [Fact]
        public void AccessPathsLevel()
        {
            AccessCheck ac = new AccessCheck();

            List<string> ids = new List<string>() { "fw.de.he.da.mkk.mtl", "fw.de.he.da.mkk.nid" };
            List<EmergenyObjectAccessContainer> acl = new List<EmergenyObjectAccessContainer>();

            var oa = new EmergenyObjectAccessContainer("fw.de.he.da.mkk");
            oa.AddAccessRight("stamm", AccessLevelEnum.Read);
            acl.Add(oa);
            Assert.Equal<AccessLevelEnum>(AccessLevelEnum.Read, ac.CheckAccessForPath("stamm", ids, acl));
        }

        [Fact]
        public void AccessPaths()
        {
            AccessCheck ac = new AccessCheck();

            List<string> ids = new List<string>() { "fw.de.he.da.mkk.mtl", "fw.de.he.da.mkk.nid" };

            List<EmergenyObjectAccessContainer> acl = new List<EmergenyObjectAccessContainer>();

            var oa = new EmergenyObjectAccessContainer("fw.de.he.da.mkk");
            oa.AddAccessRight("stamm", AccessLevelEnum.Read);
            oa.AddAccessRight("status", AccessLevelEnum.Read);
            oa.AddAccessRight("*", AccessLevelEnum.Read);
            acl.Add(oa);

            var res = ac.GetPathsByAccessLevel(AccessLevelEnum.Read, ids, acl);
            Assert.Contains<string>("*", res);
        }
    }
}
