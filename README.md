# user-ops

## Exira.Users

Exira.Users is the main application, designed as an API.

It uses [FSharp.Configuration](http://fsprojects.github.io/FSharp.Configuration/) to read configuration out of the [Web.yaml](https://github.com/exira/user-ops/blob/master/src/Exira.Users/Web.yaml) file.

Currently the [OWIN pipeline](https://github.com/exira/user-ops/blob/master/src/Exira.Users/Startup.fs) implements Web API and [GetEventStore](https://geteventstore.com/).

Each controller method takes a command and hands it off to [the domain](#Exira.Users.Domain).

## Cloning

```git clone git@gitlab.com:exira/user-ops.git -c core.autocrlf=input```
