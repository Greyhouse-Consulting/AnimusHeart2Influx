
AnimusHeart2Influx transports data from a animusheart instance to influxdb. 


### Install influx and graphana

https://www.qamilestone.com/post/end-to-end-steps-to-set-up-influxdb-grafana-using-docker-compose


### AnimusHeart limits

Animus heart allows max 7000 messages per 24h over websockets. When continusly receiving this messages over websockets the limit ot 7000 will be reached before 24 h and leaving the rest of the unmonitored. AninusHeart2influx will split the 24 h periof into 5 min slots. Intotal of 288 slots in 24 h (12 * 24). When the maximum messages are received per slot (7000/288 = 24.3) the application will pause until the next slot i started.


## How to install 

Clone this repository 

Modify the apppsettings.json with your values.

```
{
  "AnimusKey": "afeafccec0....",
  "AnimusUrl": "ws://...",
  "InfluxDbUrl": "http://...:8086/",
  "MaxWebSocketMessagesPerHour": 7000
}
```

### Build 

from the root of the repository run

`docker build .\AnimusHeart2Influx\`

This will generate a imnage id

```
...
Successfully built 58d9f0461f56 
...
```

imageid = 58d9f0461f56

### Run

`docker start {imageid}`


