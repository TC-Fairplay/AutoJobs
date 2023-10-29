# Meteoswiss API

## Weather Prognosis

### Prognosis for next 4 days (including the current one)

- URL for postal code 8057: https://app-prod-ws.meteoswiss-app.ch/v1/plzDetail?plz=805700
- format: JSON
- all absolute times are specified as Unix timestamps
- [sample file](meteoswiss-prognosis-sample.json)

#### JSON Paths of Interest

- `$.forecast[0].dayDate`: current Date
- `$.graph.temperatureMean1h[0:143]`: Average temperature for the next 4 days (4 * 24h = 144h)
  - likewise `temperatureMin1h` and `temperatureMax1h`

#### Contents

| Element       | Resolution   | Value(s)   | Unit   |
| -------          | ---------      | ---------- | ----    |
| Termperature | 1h | Min / Max / Average | Degree Celsius |
| Rain | 1h | Min / Max / Average(?) | mm/h? |
| Rain | 10min | Min / Max / Average(?) | mm/h? |
| Wind Speed | 3h | | km/h |
| Wind Direction | 3h | | Degrees |
| Sunrise | | Time | Unix Timestamp |
| Sunset | | Time | Unix Timestamp |











