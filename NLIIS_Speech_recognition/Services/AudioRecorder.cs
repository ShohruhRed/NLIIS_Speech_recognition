using System;
using NAudio.Wave;

namespace NLIIS_Speech_recognition.Services
{
    public class AudioRecorder
    {
        private WaveIn _sourceStream;
        private WaveFileWriter _waveWriter;
        private readonly int _inputDeviceIndex;

        public AudioRecorder()
        {
            _inputDeviceIndex = GetMicDeviceId();
        }
        
        private int GetMicDeviceId()
        {
            var waveInDevices = WaveIn.DeviceCount;
            
            for (var waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                var deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                
                if (deviceInfo.ProductName.Contains("Microphone"))
                {
                    return waveInDevice;
                }
            }
            
            return 0;
        }

        public void StartRecording(object sender, EventArgs e)
        {
            _sourceStream = new WaveIn
            {
                DeviceNumber = _inputDeviceIndex,
                WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(_inputDeviceIndex).Channels)
            };

            _sourceStream.DataAvailable += SourceStreamDataAvailable;

            _waveWriter = new WaveFileWriter("D:\\command.wav", _sourceStream.WaveFormat);
            _sourceStream.StartRecording();
        }

        private void SourceStreamDataAvailable(object sender, WaveInEventArgs e)
        {
            if (_waveWriter == null)
            {
                return;
            }

            _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            _waveWriter.Flush();
        }

        public void StopRec(object sender, EventArgs e)
        {
            if (_sourceStream != null)
            {
                _sourceStream.StopRecording();
                _sourceStream.Dispose();
                _sourceStream = null;
            }
            
            if (_waveWriter == null)
            {
                return;
            }
            
            _waveWriter.Dispose();
            _waveWriter = null;
        }
    }
}
