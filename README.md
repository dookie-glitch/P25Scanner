<<<<<<< HEAD
![Trunk Recorder](./docs/media/trunk-recorder-header.png)
---
[![Discord](./docs/media/discord.jpg)](https://discord.gg/btJAhESnks) &nbsp;&nbsp;


## Sponsors
**Do you find Trunk Recorder and OpenMHz useful?** 
Become a [Sponsor](https://github.com/sponsors/robotastic) to help support continued development and operation!
Thank you to everyone who has contributed!

## 🎉 V5.0 Our Best Release Yet!!
Thanks to everyone who contributed, tested and helped collect cored dumps! 

## Overview
Need help? Got something working? Share it!


- [Discord Server](https://discord.gg/btJAhESnks) 

- [Documentation](https://trunkrecorder.com/docs/intro)

- ... and don't forget the [Wiki](https://github.com/robotastic/trunk-recorder/wiki)

![screenshot](./docs/media/screenshot.jpg)

Trunk Recorder is able to record the calls on trunked and conventional radio systems. It uses 1 or more Software Defined Radios (SDRs) to do this. The SDRs capture large swathes of RF and then use software to process what was received. [GNU Radio](https://gnuradio.org/) is used to do this processing because it provides lots of convenient RF blocks that can be pieced together to allow for complex RF processing. The libraries from the amazing [OP25](http://op25.osmocom.org/trac/wiki) project are used for a lot of the P25 functionality. Multiple radio systems can be recorded at the same time.


Trunk Recorder currently supports the following:

 - Trunked P25 & SmartNet Systems
 - Conventional P25, DMR & analog systems, where each talkgroup has a dedicated RF channel
 - P25 Phase 1, P25 Phase 2 & Analog voice channels

### Supported platforms

- **Ubuntu** (18.04,  20.04, 21.04, 22.04, 23.04) 
- **Raspberry Pi** (Raspberry OS/Raspbian & Ubuntu 21.04, 22.04) 
- **Arch Linux** (2021.09.20)
- **Debian** (9.x)
- **macOS**

GNU Radio 3.7 - 3.10

### SDRs

RTL-SDR dongles; HackRF; Ettus USRP B200, B210, B205; BladeRF; Airspy; SDRplay


## Install

### Linux
- [Docker](docs/Install/INSTALL-DOCKER.md) 
- [From Source](docs/Install/INSTALL-LINUX.md)

### Raspberry Pi
- [Docker](docs/Install/INSTALL-DOCKER.md) 
- [From Source](docs/Install/INSTALL-PI.md) - [Video Walkthrough](https://youtu.be/DizBtDZ6kE8)

### MacOS
- [From Source](docs/Install/INSTALL-MAC.md#using-homebrew)



## Setup
* [Configuring a system](docs/CONFIGURE.md)
* [Uploading to OpenMHz](./docs/OpenMHz.md)
* [FAQ](docs/FAQ.md)


### Playback & Sharing
By default, Trunk Recorder just dumps a lot of recorded files into a directory. Here are a couple of options to make it easier to browse through recordings and share them on the Internet.
* [OpenMHz](https://github.com/robotastic/trunk-recorder/wiki/Uploading-to-OpenMHz): This is my free hosted platform for sharing recordings
* [Trunk Player](https://github.com/ScanOC/trunk-player): A great Python based server, if you want to you want to run your own
* [Rdio Scanner](https://github.com/chuot/rdio-scanner): Provide a good looking, scanner style interface for listening to Trunk Recorder
* Broadcastify Calls (API): see Radio Reference [forum thread](https://forums.radioreference.com/threads/405236/) and [wiki page](https://wiki.radioreference.com/index.php/Broadcastify-Calls-Trunk-Recorder)
* [Broadcastify via Liquidsoap](https://github.com/robotastic/trunk-recorder/wiki/Streaming-online-to-Broadcastify-with-Liquid-Soap)
* [audioplayer.php](https://github.com/robotastic/trunk-recorder/wiki/Using-audioplayer.php)
* [rosecitytransit's Live Web page](https://github.com/rosecitytransit/trunk-recorder-daily-log)

### Plugins

* [MQTT Status](https://github.com/robotastic/trunk-recorder-mqtt-status): Publishes the current status of a Trunk Recorder instance over MQTT
* [MQTT Statistics](https://github.com/robotastic/trunk-recorder-mqtt-statistics): Publishes statistics about a Trunk Recorder instance over MQTT
* [Decode rates logger](https://github.com/rosecitytransit/trunk-recorder-decode-rate): Logs trunking control channel decode rates to a CSV file, and includes a PHP file that outputs an SVG graph
* [Daily call log and live Web page](https://github.com/rosecitytransit/trunk-recorder-daily-log): Creates a daily log of calls (instead of just individual JSON files) and includes an updating PHP Web page w/audio player
* [Prometheus exporter](https://github.com/USA-RedDragon/trunk-recorder-prometheus): Publishes statistics to a metrics endpoint via HTTP

### Troubleshooting

If are having trouble, check out the [FAQ](docs/FAQ.md) and/or ask a question on the [Discord Server](https://discord.gg/trunk-recorder) 


## How Trunking Works
For those not familiar, trunking systems allow a large number of user groups to share a limited number of radio frequencies by temporarily, dynamically assigning radio frequencies to talkgroups (channels) on-demand. It is understood that most user groups actually use the radio very sporadically and don't need a dedicated frequency. 

Most trunking system types (such as SmartNet and P25) set aside one of the radio frequencies as a "control channel" that manages and broadcasts radio frequency assignments. When someone presses the Push to Talk button on their radio, the radio sends a message to the system which then assigns a voice frequency and broadcasts a Channel Grant message about it on the control channel. This lets the radio know what frequency to transmit on and tells other radios set to the same talkgroup to listen.

In order to follow all of the transmissions, Trunk Recorder constantly listens to and decodes the control channel. When a frequency is granted to a talkgroup, Trunk Recorder creates a monitoring process which decodes the portion of the radio spectrum for that frequency from the SDR that is already pulling it in.

No message is transmitted on the control channel when a conversation on a talkgroup is over. The monitoring process keeps track of transmissions and if there has been no activity for a specified period, it ends the recording.
=======
# P25Scanner

A software-defined radio (SDR) based P25 digital voice scanner application.

## Features

- Real-time P25 Phase 1 digital voice decoding
- Talkgroup filtering and monitoring
- Signal quality metrics and optimization
- User-friendly interface for radio control
- Automatic gain and frequency correction
- Comprehensive error handling and logging

## Prerequisites

- Windows 10 or later
- .NET 7.0 SDK or later
- Compatible SDR receiver (RTL-SDR, SDRPlay, etc.)
- SDR device drivers installed

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/P25Scanner.git
cd P25Scanner
```

2. Install dependencies:
```bash
dotnet restore
```

3. Build the application:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

## Usage

1. Start the application using the provided script or `dotnet run`
2. Select your SDR device from the dropdown menu
3. Enter the frequency you want to monitor
4. Adjust squelch threshold if needed
5. Click "Start" to begin scanning
6. Use the talkgroup panel to filter specific talkgroups

### Configuration

The application settings can be modified in the `appsettings.json` file:

- `SampleRate`: SDR sample rate (default: 2.048 MSPS)
- `SquelchThreshold`: Default squelch threshold (dB)
- `EnableLogging`: Enable/disable detailed logging
- `LogLevel`: Logging detail level

## Troubleshooting

### Common Issues

1. **No SDR Device Found**
   - Ensure SDR is properly connected
   - Check device drivers are installed
   - Try different USB ports
   - Restart the application

2. **Poor Signal Quality**
   - Check antenna connection
   - Adjust gain settings
   - Move antenna for better reception
   - Monitor signal metrics in application

3. **No Audio Output**
   - Verify system audio settings
   - Check audio device selection
   - Ensure squelch isn't set too high
   - Verify P25 signal is present

4. **Application Crashes**
   - Check log files in `logs` directory
   - Verify .NET runtime is installed
   - Ensure all dependencies are installed
   - Try rebuilding the application

### Logging

Log files are stored in the `logs` directory with the following format:
- `p25scanner-YYYY-MM-DD.log`: Daily application log
- `error-YYYY-MM-DD.log`: Error log

To enable detailed logging, set `EnableLogging` to `true` in `appsettings.json`.

### Performance Optimization

For optimal performance:
1. Use recommended sample rate (2.048 MSPS)
2. Monitor CPU usage
3. Keep SDR device cool
4. Use quality antenna
5. Monitor signal metrics

## Support

For bug reports and feature requests, please submit an issue on the GitHub repository.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Acknowledgments

- DVSI for IMBE vocoder specifications
- P25 standards documentation
- SDR community resources

>>>>>>> 9393c7a4daab2a89b5ba644b8048bfb4dc45526c
