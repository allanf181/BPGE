### Disclaimer

This project is not endorsed by or affiliated with Overwolf or Home Assistant.

# ButtPlug Game Events

ButtPlug integration for Apex Legends, CS2, Dota 2, Fortnite, Overwatch, PUBG, Rainbow Six Siege, Rocket League, Team Fortress 2, and more! [Full list here](https://overwolf.github.io/api/live-game-data)

## Installation

### Requirements
 - BPGE (obviously)
 - Intiface Central
 - Overwolf
 - Overwolf app [Home Assistant Game Events](https://www.overwolf.com/app/BinaryBurger-HomeAssistant_Game_Events)

### Setup
1. Launch Intiface® Central
2. Start the server and configure your devices
3. Set the config file for BPGE in same folder as the executable, you can use the global config available in this repo
4. Launch BPGE and click in the server address to copy it to your clipboard
5. Launch Overwolf and open the Home Assistant Game Events app
6. Paste the server address into Webhook URL, set Throttle to 1 and click on Save
7. Restart Overwolf (optional, but recommended)
8. Launch your game and enjoy!

### Config file format
Can be .yml or .yaml, global config is executed in all games, except if specific config for game overrides it.

The list of events can be found [here](https://overwolf.github.io/api/live-game-data).

global config format `global.yml`:
```yaml
events:
  event_name: # event name, needs to be the same as in api
    intensity: 50 # 0-100
    duration: 5 # seconds, decimal values with 1 decimal place are allowed
  # setting intensity to 0 will stop vibrations 
  # setting duration to 0 will make the vibration last forever (5 minutes) or until a event with 0 intensity is sent, useful for games that you need to be revived
  event_name2:
    intensity: 80
    duration: 5
```

game specific config format, the file name needs to be the same as the game id, for example Apex Legends: `21566.yml`
```yaml
mode: append # append or override, append will add the events to the global config, override will replace the global config with the game specific config
events:
  event_name: # event name, needs to be the same as in api
    intensity: 50 # 0-100
    duration: 5 # seconds, decimal values with 1 decimal place are allowed
```