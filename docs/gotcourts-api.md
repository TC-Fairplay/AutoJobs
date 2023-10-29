# GotCourts API

## Blocking

All HTTP request sent to the GotCourts API have to contain the following headers for authentication:

- `X-GOTCOURTS: ApiKey="[secret]"`
- `Cookie: PHPSESSID=[secret]`

The secret codes are assigned on manual login to the GotCourts administration (extract them manually from the HTTP data) and seem to remain valid almost forever.

### IDs

- TC Fairplay: 53223
- Court 1: 8153
- Court 2: 8154
- Court 3: 8155

### Encodings

#### Date

- Format: Year-Month-Day
- Example: 2023-10-09

#### Time

- Format: # Seconds since midnight
- Examples:
  - 28000 (= 8.00 o'clock, 8 * 3600 = 28'000)
  - 43200 (= 12.00 o'clock, 12 * 3600 = 43'200)

### Create a blocking

Note: Parameter `autoremove` has to be set to `true` so that existing overlapping reservations are removed automatically.

- URL: https://apps.gotcourts.com/de/api2/secured/club/blocking
- Method: POST
- Parameters (with sample data):

```text
type=other
description=Bodenfrost
courts[]=8153
courts[]=8154
courts[]=8155
date=2023-10-09
dateTo=2023-10-09
time[start]=28800 // 08.00
time[end]=43200   // 12.00
allDay[disabled]=true
note=
autoremove=true
```

Response (on success):

```json
{"status":true,"response":{"reservations":0,"autoremove":true,"ids":["8a8b1622-690d-11ee-8eb4-0a838c1c911b"]},"version":"3.27.0.0"}
```

cURL Code Sample:

```sh
curl 'https://apps.gotcourts.com/de/api2/secured/club/blocking' \
  -X POST \
  -H 'X-GOTCOURTS: ApiKey="[secret]"' \
  -H 'Cookie: PHPSESSID=[secret]' \
  --data-raw 'type=other&description=Test&courts%5B%5D=8153&date=2023-10-09&dateTo=2023-10-09&time%5Bstart%5D=28800&time%5Bend%5D=32400&allDay%5Bdisabled%5D=true&note=&autoremove=true'
```

### Delete a blocking

- URL: https://apps.gotcourts.com/de/api2/secured/club/blocking/8a8b1622-690d-11ee-8eb4-0a838c1c911b (the GUID at the end of the URL identifies the previously defined blocking)
- Method: DELETE

Response (on success):

```json
{"status":true,"response":{"canceled":true,"blockId":"8a8b1622-690d-11ee-8eb4-0a838c1c911b"},"version":"3.27.0.0"}
```

cURL Code Sample

```sh
curl 'https://apps.gotcourts.com/de/api2/secured/club/blocking/3767f7fd-682b-11ee-8eb4-0a838c1c911b' \
  -X DELETE \
  -H 'X-GOTCOURTS: ApiKey="[secret]"' \
  -H 'Cookie: PHPSESSID=[secret]'
```

### Further URLs

- https://apps.gotcourts.com/de/api/secured/club/reservation/list?clubId=53223&date=2023-10-10
- https://apps.gotcourts.com/de/api/public/clubs/53223/reservations?date=2023-10-12 (public)
