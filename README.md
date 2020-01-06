
# Emergency Data Exchange Protocol

Bei dem Emergency Data Exchange Protocol (EDXP) handelt es sich um eine Spezifikation, basierend auf dem REST Protokoll, zum Austausch von Einsatzdaten zwischen Beh�rden der Gefahrenabwehr (z.B. Feuerwehr, Polizei, Rettungsdienst) �ber eine zentrale Dateninstanz. In Zukunft sollen Daten�nderungen in Echtzeit �ber ein Websocketprotokoll an die angeschlossenen Organisationen verteilt werden k�nnen. (Siehe Roadmap)
Das Protokoll wurde auf Initiative von REV Plus (www.einsatzverwaltung.de) entwickelt und mit anderen Softwareherstellern abgestimmt.

## Objektspeicher
Grunds�tzlich werden s�mtliche Daten als Objekte mit einer eindeutigen UID im Cloudspeicher abgelegt. Jedes dieser Objekte hat ein Header f�r Metadaten und die Zugriffssteuerung. Der Body des Datenobjektes enth�lt die entsprechenden Nutzdaten. Diese Nutzdaten werden fachlich als sogenannte Emergency-Objects in unterschiedliche Datentypen unterteilt.
### Datentypen
Im Protokoll werden unterschiedliche fachliche Datentypen spezifiziert. Diese enthalten neben der entsprechenden Datenfelder auch Feldtypen und Validierungsregeln die f�r eine einheitliche Verwendung der Datenfelder �ber alle angeschlossenen System sorgen soll.
Datentypen k�nnen aufgrund eines Vorschlages in das Protokoll aufgenommen werden. Aktuell existieren die folgenden Datentypen:
- Einsatz
- Einsatzmittel
### Header
Der Header eines Datenobjektes enth�lt Metadaten als Zusatzinformationen zu den eigentlichen Nutzdaten. Diese Metadaten k�nnen nur in bestimmten F�llen durch einen Anwender modifiziert werden. Im Einzelnen handelt es sich um folgende Felder:

- Zeitpunkt und UID Referenz auf den Ersteller des Objektes
- Zeitpunkt und UID Referenz auf den Account der letzten �nderung
- Access Control List zur Zugriffssteuerung
- TimeToLive - Zeit in Minuten, die ein Datensatz existieren soll
- Datentyp der Nutzdaten

Den Datentyp, die Zeitpunkte und UID Referenzen �ndert ausschlie�lich der Server, bzw. werden automatisch beim Anlegen / Updaten gesetzt. Die �nderung der Access Control List ist �ber einen REST Endpunkt m�glich - kann jedoch nur von den Systemen ge�ndert werden, die Besitzer des Objektes oder das GRANT Recht f�r das Objekt besitzen.
### Access Identity
Jede angeschlossene Organisation erh�lt durch den Administrator einen Account der mit einem oder mehreren Access-Identities ausgestattet ist. Anhand der Access Identity wird der Zugriff auf die Emergency-Objects kontrolliert. Die Access Identity bildet dabei die organisatorische sowie geopolitische Gliederung der Organisation ab. Sie ist nach folgendem Format (Am Beispiel Deutschland) aufgebaut:

    <Organisation>.<Kontinent>.<Land>.<Bundesland>.<RP>.<Landkreis>.<Stadt>.<WeitereUntergliederung>

Mit entsprechenden Reg-Ex Pattern ist es somit m�glich einem bestimmten Organisationskreis Zugriff auf bestimmte Daten zu erm�glichen und wiederrum andere Organisationen oder geografische Gebiete auszuschlie�en. Die entsprechenden Reg-Ex Pattern werden in der Access-Control-List zur Zuteilung der Berechtigungsstufen verwendet.
### Berechtigungsstufen
Grunds�tzlich kann �ber die Access-Control-List ein Zugriff auf ein bestimmtes Element eines Emergency-Objects oder sogar das gesamte Objekt eingeschr�nkt oder erlaubt werden. Dabei stehen folgende Zugriffslevel zur Verf�gung:

- Keine
- Leserechte (erm�glicht nur das Lesen einer Ressource)
- Schreibrechte (Erm�glicht das Ver�ndern der vorhandenen Daten)
- Grant-Rechte (Erm�glicht das Ver�ndern der Access-Control-List f�r diesen Nutzer, dies schlie�t auch die Schreibrechte mit ein)
- Besitzer (Vollzugriff auf das Objekt)

�ber die Angabe von Pfaden ist es m�glich Berechtigungen auf einen Teil des Objektes zu setzen. Dabei stellt der Stern `*` den Zugriff auf alle Elemente dar. Ansonsten k�nnen Elementpfade im Format `status.position.altitude` angegeben werden. Pfade werden immer klein geschrieben und Kind-Pfade mit einem Punkt (.) vom Elternpfad getrennt.
## API Zugriff
Der Zugriff auf die API erfolgt mittes HTTPS und einem API Key. Dieser API Key kann �ber den Account-Controller angelegt werden. Es ist durchaus m�glich und erw�nscht, dass eine Organisation mehrerer API Keys verwendet, wenn auch mehrere Systeme daran beteiligt sind. Der API Key wird als Teil des HTTP-Headers mitgesendet. Bei folgendem Beispiel wird bei einer Post Anfrage im Header der Authorization Header mitgesendet. Keine Auswirkung hat der API Key auf die Access Identity. Diese ist fest mit dem Account verkn�pft und gilt f�r alle API Keys des Accounts.

`POST https://test.einsatzverwaltung.de/v1/Objects/`
`Host: test.einsatzverwaltung.de`
`Authorization: Bearer <API Key>`

Eine genaue Erl�uterung der API M�glichkeiten gibt das aktuelle Swaggerfile. Mit Hilfe der OpenAPI Spezifikation k�nnen Sie leicht den Client-Code f�r Ihre Integration generieren lassen.
# Beispiele
Im folgenden werden ein paar g�ngige Beispiele aufgezeigt, die die Nutzung des Protokolls greifbar machen sollen. Der Fokus liegt dabei an dem Anlegen und Updaten von Emergency-Objects. Die Verwaltung der Accounts und andere Managementfunktionen werden hier nicht betrachtet. Hier kann die Open API Dokumentation zu Rate gezogen werden.
## Anlegen eines neuen Objektes (mit ID)
Soll ein neues Objekt angelegt werden so kann bereits in dem eigenen System eine zuf�llige UID generiert und vergeben werden. Diese UID wird dann durch das EDXP System �bernommen - sofern es keine Kollision mit einem bestehenden Datensatz gibt.
#### Request
`POST https://test.einsatzverwaltung.de/v1/Objects/`
`Authorization: Bearer <API Key>`
``

## Anlegen eines neuen Objektes (ohne ID)

# Roadmap
Wir begr��en die Teilnahme an der Entwicklung weiterer Features dieses Protokolls. Ein paar Ideen sind bereits hier zusammengefasst. Bitte erstelle ein Fork vom Repository und beginne dort die Entwicklung. Am Ende wird dies auf unserem GIT Server zusammengefasst.

 - Verteilung von Daten�nderungen an berechtigte Empf�nger �ber Websocket Protokoll
 - Erstellen einer Administrations UI zum Verwalten der Accounts und Datenobjekte
