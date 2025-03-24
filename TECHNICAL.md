# P25Scanner Technical Documentation

## Architecture Overview

P25Scanner is built using .NET 7.0 and implements a P25 Phase 1 digital voice decoder. The application is structured into several key components:

### Core Components

1. **Signal Processing (P25DSP)**
   - Root-raised cosine filtering
   - Symbol timing recovery
   - QPSK demodulation
   - Frame synchronization

2. **Error Correction (GolayCode)**
   - Golay(24,12) error correction
   - Syndrome calculation
   - Error pattern lookup
   - Bit error rate tracking

3. **Voice Decoding (IMBEDecoder)**
   - IMBE parameter extraction
   - Spectral amplitude calculation
   - Voice synthesis
   - Frame processing

4. **Signal Metrics (P25Metrics)**
   - SNR calculation
   - Sync correlation tracking
   - Frequency offset estimation
   - Performance monitoring

### Class Hierarchy

```
P25Scanner.Services
├── IP25Decoder (interface)
├── P25Decoder
├── P25DSP
├── GolayCode
├── IMBEDecoder
└── P25Metrics

P25Scanner.Services.Models
├── DecoderStatus
├── DecoderStatusEventArgs
├── DecodedAudioEventArgs
└── TalkgroupEventArgs
```

## Development Setup

### Required Tools

- Visual Studio 2022 or later / VS Code
- .NET 7.0 SDK
- Git
- SDR development tools

### Building

1. **Debug Build**
   ```bash
   dotnet build --configuration Debug
   ```

2. **Release Build**
   ```bash
   dotnet build --configuration Release
   ```

### Testing

Run unit tests:
```bash
dotnet test
```

### Development Guidelines

1. **Code Style**
   - Follow C# coding conventions
   - Use XML documentation comments
   - Implement proper exception handling
   - Add logging for important operations

2. **Performance Considerations**
   - Minimize allocations in signal processing
   - Use efficient algorithms for real-time processing
   - Profile code for bottlenecks
   - Monitor memory usage

3. **Signal Processing**
   - Sample rate: 2.048 MSPS
   - Symbol rate: 4800 symbols/sec
   - Filter roll-off: 0.2
   - Frame size: 384 symbols

## Debugging

### Common Debug Points

1. **Signal Processing**
   - Check signal levels with CalculateSNR
   - Monitor sync correlation values
   - Verify frequency offset estimation

2. **Voice Decoding**
   - Validate IMBE parameter extraction
   - Check voice frame reconstruction
   - Monitor audio output quality

3. **Error Correction**
   - Track bit error rates
   - Verify syndrome calculation
   - Monitor correction success rate

### Performance Profiling

Use Visual Studio profiler or dotnet-trace:
```bash
dotnet-trace collect --process-id <PID>
```

### Logging

The application uses structured logging with different levels:
- Error: Critical issues
- Warning: Potential problems
- Information: Normal operation
- Debug: Detailed diagnostics
- Trace: Signal processing details

## Deployment

### Release Process

1. Update version in project file
2. Run all tests
3. Build release configuration
4. Create release package
5. Update documentation

### Configuration

The application uses appsettings.json for configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "SDR": {
    "SampleRate": 2048000,
    "DefaultGain": 30,
    "SquelchThreshold": -30
  },
  "Decoder": {
    "EnableMetrics": true,
    "MetricsInterval": 1000
  }
}
```

## Maintenance

### Performance Monitoring

Monitor these metrics:
- CPU usage
- Memory consumption
- Frame processing time
- Error correction rate
- Signal quality metrics

### Troubleshooting

1. Check log files
2. Monitor signal metrics
3. Verify SDR device operation
4. Test audio output chain
5. Validate configuration settings

## API Documentation

### IP25Decoder Interface

```csharp
public interface IP25Decoder : IDisposable
{
    bool IsActive { get; }
    float SquelchThreshold { get; set; }
    float SignalQuality { get; }
    int CurrentTalkgroupId { get; }
    int CurrentNAC { get; }

    Task<bool> InitializeAsync(uint sampleRate);
    Task<bool> StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
    Task<bool> ProcessIQDataAsync(Complex[] iqData);
    void AddTalkgroupFilter(int talkgroupId);
    void RemoveTalkgroupFilter(int talkgroupId);
    void ClearTalkgroupFilters();

    event EventHandler<DecodedAudioEventArgs> DecodedAudioAvailable;
    event EventHandler<TalkgroupEventArgs> TalkgroupDetected;
    event EventHandler<DecoderStatusEventArgs> DecoderStatusChanged;
}
```

For detailed API documentation, see XML comments in source code.

