using EmergencyDataExchangeProtocol.Datastore;
using System;
using Xunit;

namespace Test
{
    public class AccessCheckTests
    {
        [Fact]
        public void Identifiers()
        {
            AccessCheck ac = new AccessCheck();
            // Gilt f�r alle Feuerwehren Deutschlands - Matcht Feuerwehr Main-Kinzig
            Assert.True(ac.IdentifierMatches("fw.de.he.da.mkk", "fw.de"));
            // Gilt f�r Feuerwehr GElnhausen, Feuerwehr Maintal darf nicht matchen
            Assert.False(ac.IdentifierMatches("fw.de.he.da.mkk.mtl", "fw.de.he.da.mkk.gn"));
            // Gilt f�r alle Organisationen im MKK
            Assert.True(ac.IdentifierMatches("fw.de.he.da.mkk.mtl", "?.de.he.da.mkk"));
        }
    }
}
