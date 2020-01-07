
# Emergency Data Exchange Protocol
Bei dem Emergency Data Exchange Protocol (EDXP) handelt es sich um eine Spezifikation, basierend auf dem REST Protokoll, zum Austausch von Einsatzdaten zwischen Behörden der Gefahrenabwehr (z.B. Feuerwehr, Polizei, Rettungsdienst) über eine zentrale Dateninstanz. In Zukunft sollen Datenänderungen in Echtzeit über ein Websocketprotokoll an die angeschlossenen Organisationen verteilt werden können. (Siehe Roadmap)
Das Protokoll wurde auf Initiative von REV Plus (www.einsatzverwaltung.de) entwickelt und mit anderen Softwareherstellern abgestimmt.

## Objektspeicher
Grundsätzlich werden sämtliche Daten als Objekte mit einer eindeutigen UID im Cloudspeicher abgelegt. Jedes dieser Objekte hat ein Header für Metadaten und die Zugriffssteuerung. Der Body des Datenobjektes enthält die entsprechenden Nutzdaten. Diese Nutzdaten werden fachlich als sogenannte Emergency-Objects in unterschiedliche Datentypen unterteilt.

### Datentypen
Im Protokoll werden unterschiedliche fachliche Datentypen spezifiziert. Diese enthalten neben der entsprechenden Datenfelder auch Feldtypen und Validierungsregeln die für eine einheitliche Verwendung der Datenfelder über alle angeschlossenen System sorgen soll.
Datentypen können aufgrund eines Vorschlages in das Protokoll aufgenommen werden. Aktuell existieren die folgenden Datentypen:
- Einsatz
- Einsatzmittel

Um eigene Datentypen vorzuschlagen muss ein Pull Request zu dem original EDXP GIT Repository mit den gewünschten Änderungen gemacht werden. Nach Diskussion des Pull Requests werden die Änderungen zusammengeführt.

### Header
Der Header eines Datenobjektes enthält Metadaten als Zusatzinformationen zu den eigentlichen Nutzdaten. Diese Metadaten können nur in bestimmten Fällen durch einen Anwender modifiziert werden. Im Einzelnen handelt es sich um folgende Felder:

- Zeitpunkt und UID Referenz auf den Ersteller des Objektes
- Zeitpunkt und UID Referenz auf den Account der letzten Änderung
- Access Control List zur Zugriffssteuerung
- TimeToLive - Zeit in Minuten, die ein Datensatz existieren soll
- Datentyp der Nutzdaten

Den Datentyp, die Zeitpunkte und UID Referenzen ändert ausschließlich der Server, bzw. werden automatisch beim Anlegen / Updaten gesetzt. Die Änderung der Access Control List ist über einen REST Endpunkt möglich - kann jedoch nur von den Systemen geändert werden, die Besitzer des Objektes oder das GRANT Recht für das Objekt besitzen.

### Access Identity
Jede angeschlossene Organisation erhält durch den Administrator einen Account der mit einem oder mehreren Access-Identities ausgestattet ist. Anhand der Access Identity wird der Zugriff auf die Emergency-Objects kontrolliert. Die Access Identity bildet dabei die organisatorische sowie geopolitische Gliederung der Organisation ab. Sie ist nach folgendem Format (Am Beispiel Deutschland) aufgebaut:

    <Organisation>.<Kontinent>.<Land>.<Bundesland>.<RP>.<Landkreis>.<Stadt>.<WeitereUntergliederung>

Mit entsprechenden Reg-Ex Pattern ist es somit möglich einem bestimmten Organisationskreis Zugriff auf bestimmte Daten zu ermöglichen und wiederrum andere Organisationen oder geografische Gebiete auszuschließen. Die entsprechenden Reg-Ex Pattern werden in der Access-Control-List zur Zuteilung der Berechtigungsstufen verwendet.
### Berechtigungsstufen
Grundsätzlich kann über die Access-Control-List ein Zugriff auf ein bestimmtes Element eines Emergency-Objects oder sogar das gesamte Objekt eingeschränkt oder erlaubt werden. Dabei stehen folgende Zugriffslevel zur Verfügung:

- Keine
- Leserechte (ermöglicht nur das Lesen einer Ressource)
- Schreibrechte (Ermöglicht das Verändern der vorhandenen Daten)
- Grant-Rechte (Ermöglicht das Verändern der Access-Control-List für diesen Nutzer, dies schließt auch die Schreibrechte mit ein)
- Besitzer (Vollzugriff auf das Objekt)

Über die Angabe von Pfaden ist es möglich Berechtigungen auf einen Teil des Objektes zu setzen. Dabei stellt der Stern `*` den Zugriff auf alle Elemente dar. Ansonsten können Elementpfade im Format `status.position.altitude` angegeben werden. Pfade werden immer klein geschrieben und Kind-Pfade mit einem Punkt (.) vom Elternpfad getrennt.
## API Zugriff
Der Zugriff auf die API erfolgt mittes HTTPS und einem API Key. Dieser API Key kann über den Account-Controller angelegt werden. Es ist durchaus möglich und erwünscht, dass eine Organisation mehrerer API Keys verwendet, sofern mehrere Systeme an der Kommunikation beteiligt sind. (Es soll also pro angeschlossenem System eines Teilnehmers, ein API Key erstellt werden) Der API Key wird als Teil des HTTP-Headers mitgesendet. Bei folgendem Beispiel wird bei einer Post Anfrage im Header der Authorization Header mitgesendet. Keine Auswirkung hat der API Key auf die Access Identity. Diese ist fest mit dem Account verknüpft und gilt für alle API Keys des Accounts.

```
POST https://test.einsatzverwaltung.de/v1/Objects/
Host: test.einsatzverwaltung.de
Authorization: Bearer <API Key>
```

Eine genaue Erläuterung der Möglichkeiten der REST API gibt das aktuelle Swaggerfile. (Referenzimplementierung: https://edxp.bosmesh.org/swagger/v1/swagger.json) Mit Hilfe der OpenAPI Spezifikation können Sie leicht den Client-Code für Ihre Integration generieren lassen. Außerdem ist eine API Web UI für die aktuelle Implementierung unter https://edxp.bosmesh.org/swagger/index.html erreichbar.
# Beispiele
Im folgenden werden ein paar gängige Beispiele aufgezeigt, die die Nutzung des Protokolls greifbar machen sollen. Der Fokus liegt dabei an dem Anlegen und Updaten von Emergency-Objects. Die Verwaltung der Accounts und andere Managementfunktionen werden hier nicht betrachtet. Hier kann die Open API Dokumentation zu Rate gezogen werden.
## Anlegen eines neuen Objektes (mit ID)
Soll ein neues Objekt angelegt werden so kann bereits in dem eigenen System eine zufällige UID generiert und vergeben werden. Diese UID wird dann durch das EDXP System übernommen - sofern es keine Kollision mit einem bestehenden Datensatz gibt.
#### Request
```
POST https://test.einsatzverwaltung.de/v1/Objects/
Authorization: Bearer <API Key>
```

## Anlegen eines neuen Objektes (ohne ID)

# Websocket Live-Update
Über eine Websocketverbindung zum Server ist es möglich Aktualisierungen an Objekten automatisch nach der Änderung zu empfangen. Dazu muss eine Websocketverbindung zum Endpunkt ´wss://server/ws?key=<API Key>&since=<Zeitpunkt>´ geöffnet werden.

Dabei besteht optional die Möglichkeit durch Angabe eines Zeitpunktes der letzten Verbindung alle seit dem modifizierten Objekte zusenden zu lassen. Ist dieser Zeitpunkt angegeben, so werden nach dem Aufbau der Verbindung automatisch alle Objekte übertragen.
    
Dabei existieren zwei Parameter:
    - key => Ist der API Key, der auch für die REST Aufrufe verwendet wird. Dieser ist zwingend erforderlich.
    - since => Gibt einen Zeitpunkt im ISO-8601 Format an, wann das letzte Mal eine Verbindung bestanden hatte. 

# Roadmap
Wir begrüßen die Teilnahme an der Entwicklung weiterer Features dieses Protokolls. Ein paar Ideen sind bereits hier zusammengefasst. Bitte erstelle ein Fork vom Repository und beginne dort die Entwicklung. Am Ende wird dies auf unserem GIT Server zusammengefasst.

 - Verteilung von Datenänderungen an berechtigte Empfänger über Websocket Protokoll
 - Erstellen einer Web-Administrations UI zum Verwalten der Accounts und Datenobjekte
 - Erstellen eines CLI Clientes zu Entwicklungszwecken
