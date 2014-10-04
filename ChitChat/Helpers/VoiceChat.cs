/*
 Copyright (c) 2014 Syed Omar Raihan
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using NAudio.Wave;

namespace ChitChat
{
    delegate void OutgoingVoiceAvailableHandler(byte[] data);

    class VoiceChat
    {
        public event OutgoingVoiceAvailableHandler OutgoingVoiceAvailable;

        WaveIn waveIn;
        WaveOut waveOut;
        BufferedWaveProvider waveProvider;

        public VoiceChat()
        {
            var waveFormat = new WaveFormat(8000, 16, 1);
            waveProvider = new BufferedWaveProvider(waveFormat);

            waveOut = new WaveOut();
            waveOut.Init(waveProvider);

            waveIn = new WaveIn();
            waveIn.WaveFormat = waveFormat;

            waveIn.DataAvailable += delegate(object sender, WaveInEventArgs e)
            {
                OnOutgoingVoiceAvailable(e.Buffer);
            };
        }

        public void Start()
        {
            waveOut.Play();
            waveIn.StartRecording();
        }

        public void Stop()
        {
            waveOut.Stop();
            waveIn.StopRecording();
        }

        public void Play(byte[] data)
        {
            waveProvider.AddSamples(data, 0, data.Length);
        }

        void OnOutgoingVoiceAvailable(byte[] data)
        {
            if (OutgoingVoiceAvailable != null)
            {
                OutgoingVoiceAvailable(data);
            }
        }
    }
}
