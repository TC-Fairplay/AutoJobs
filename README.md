# Automatic Jobs

![Build Status](https://github.com/TC-Fairplay/AutoJobs/actions/workflows/build.yml/badge.svg)
![Run Status](https://github.com/TC-Fairplay/AutoJobs/actions/workflows/run.yml/badge.svg)

Automatic jobs ("cron jobs") to be run in regular intervals, written in F#.
The program needs to be hosted on a server supporting .NET console applications.

All implemented jobs are listed in [Jobs.fs](src/Jobs.fs)

## Ground Frost ❄️

### Condition

Checks weather prognosis for the upcoming night. If temperatures **drop below zero degree Celsius**, all courts will be blocked for the next day.

### Blocking Duration

If the temperature **rises above 5 degrees** during the day, the blocking stops at noon, otherwise it covers the entire day.

### External Services

* The weather prognosis is fetched from [MeteoSwiss](https://www.meteoswiss.admin.ch) (see [MeteoSwiss.fs](src/MeteoSwiss.fs)).
* Tennis courts are blocked using the [GotCourts](https://www.gotcourts.com) API (admin credentials are needed, see [GotCourts.fs](src/GotCourts.fs)).
