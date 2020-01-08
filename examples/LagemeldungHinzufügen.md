```
PATCH /object/jsonpatch/02715595-fae3-45b2-8fca-50da3de7cbf2 HTTP/1.1
Content-Type: application/json
Authorization: Bearer <API Key>
Content-Length: 100

[{
  "op":"add",
  "path":"/lagemeldungen",
  "value": {
	"zeit" : "2020-01-01T15:00:00Z",
	"meldung" : "Dies ist eine neue Meldung!"
  }
}]
```