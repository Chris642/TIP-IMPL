using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FragLabs.Audio.Codecs;
using System.Threading;
using System.Diagnostics;

namespace TIPimpl
{
    class VoiceHandling
    {
        public WaveIn waveIn = new WaveIn();
        public WaveOut waveOut = new WaveOut();
        public static BufferedWaveProvider playBuffer;
        OpusEncoder encoder = null;
        static OpusDecoder decoder = null;
        int segmentFrames;
        int bytesPerSegment;
        ulong bytesSent;
        DateTime startTime;
        Networking network;
        int sum = 0;
        static int outsum = 0;

        byte[] notEncodedBuffer = new byte[0];
        byte[] EncodedBuffer = new byte[0];
        byte[] soundBuffer = new byte[0];
        public VoiceHandling()
        {
            this.network = new Networking();
        }
        
        
        public List<String> populateIN_devices()
        {
            List<String> iNList = new List<String>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                iNList.Add(WaveIn.GetCapabilities(i).ProductName);
            }
            return iNList;
        }
        public List<String> populateOUT_devices()
        {
            List<String> oUTList = new List<String>();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                oUTList.Add(WaveOut.GetCapabilities(i).ProductName);
            }
            return oUTList;
        }


        public void Record_OPUS(int device_num, string ip)
        {
            network.Initializecon(ip);
            startTime = DateTime.Now;
            bytesSent = 0;
            segmentFrames = 960;
            encoder = OpusEncoder.Create(48000, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
            encoder.Bitrate = 8192;
            bytesPerSegment = encoder.FrameByteCount(segmentFrames);

            waveIn = new WaveIn(WaveCallbackInfo.FunctionCallback());
            waveIn.BufferMilliseconds = 50;
            waveIn.DeviceNumber = device_num;
            waveIn.DataAvailable += waveIn_DataAvailableEvent;
            waveIn.WaveFormat = new WaveFormat(48000, 16, 1);
            waveIn.StartRecording();
        }


        public void Play_OPUS(int device_num, string ip)
        {
            decoder = OpusDecoder.Create(48000, 1);
            network.Initializecon(ip);
            network.Datalisten();
            playBuffer = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
            waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            waveOut.DeviceNumber = device_num;
            waveOut.Init(playBuffer);
            waveOut.Play();
        }
            public void waveIn_DataAvailableEvent(object sender, WaveInEventArgs e)
        {
            sum = 0;
            for (int i = 0; i < 8; i++)
            {
                sum += Math.Abs(BitConverter.ToInt16(e.Buffer, 200 * i));
            }
            sum /= 8;
            MainWindow.volumein = sum;
            if (sum > MainWindow.lastmax * 0.2)
            {
                soundBuffer = new byte[e.BytesRecorded + notEncodedBuffer.Length]; //Legnht = new data + old data
                for (int i = 0; i < notEncodedBuffer.Length; i++)   //First we try encode as much as we can from old data
                    soundBuffer[i] = notEncodedBuffer[i];
                for (int i = 0; i < e.BytesRecorded; i++)
                    soundBuffer[i + notEncodedBuffer.Length] = e.Buffer[i];
            }
            else
            {
                soundBuffer = new byte[notEncodedBuffer.Length];
                for (int i = 0; i < notEncodedBuffer.Length; i++)   //First we try encode as much as we can from old data
                    soundBuffer[i] = notEncodedBuffer[i];
            }

            if (soundBuffer.Length != 0)
            {
                int byteCap = bytesPerSegment;
                int segmentCount = (int)Math.Floor((decimal)soundBuffer.Length / byteCap);
                int segmentsEnd = segmentCount * byteCap;
                int notEncodedCount = soundBuffer.Length - segmentsEnd;
                notEncodedBuffer = new byte[notEncodedCount];
                for (int i = 0; i < notEncodedCount; i++)
                {
                    notEncodedBuffer[i] = soundBuffer[segmentsEnd + i];
                }
                int len;

                for (int i = 0; i < segmentCount; i++)
                {
                    byte[] segment = new byte[byteCap];
                    for (int j = 0; j < segment.Length; j++)
                        segment[j] = soundBuffer[(i * byteCap) + j];
                    byte[] EncodedBuffer = encoder.Encode(segment, segment.Length, out len);
                    bytesSent += (ulong)len;
                    byte[] CutEncoded = new byte[len];
                    Array.Copy(EncodedBuffer, CutEncoded, len);
                    //send
                    //decode(CutEncoded, CutEncoded.Length);
                    network.Datasend(CutEncoded);
                }
            }
        }

        static public void decode(byte[] buff, int len)
        {
            buff = decoder.Decode(buff, len, out len);
            outsum = 0;
            for (int i = 0; i < 8; i++)
            {
                outsum += Math.Abs(BitConverter.ToInt16(buff, 200 * i));
            }
            outsum /= 8;
            MainWindow.volumeout = outsum;
            playBuffer.AddSamples(buff, 0, len);
        }

        public void Stop_it()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }
            network.Closeconn();
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            playBuffer = null;
            if (encoder != null)
            {
                encoder.Dispose();
                encoder = null;
            }
            if (decoder != null)
            {
                decoder.Dispose();
                decoder = null;
            }
            
        }
        public void Stop_recording()
        {
            network.Closeconn();
            waveIn.StopRecording();
            waveIn.Dispose();
            waveIn = null;
            if (encoder != null)
            {
                encoder.Dispose();
                encoder = null;
            }           
        }
        public void Stop_playing()
        {
            network.Closeconn();            
            waveOut.Stop();
            waveOut.Dispose();
            waveOut = null;
            playBuffer = null;
            if (decoder != null)
            {
                decoder.Dispose();
                decoder = null;
            }

        }


    }
}
