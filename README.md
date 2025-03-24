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

