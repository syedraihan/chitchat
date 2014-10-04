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
using System.Media;

namespace ChitChat
{
    enum SoundType
    {
        RingIn,
        RingOut,
        Disconnected,
        MessageReceived,
        BuddyOnline
    }

    class SoundManager
    {
        SoundPlayer player;

        public SoundManager()
        {
            player = new SoundPlayer();
        }

        public void Play(SoundType soundType, bool repeat)
        {
            string path = string.Empty;

            if (soundType == SoundType.RingIn)
                path = @"Resources/VoiceChatRingingIn.wav";

            else if (soundType == SoundType.RingOut)
                path = @"Resources/VoiceChatRingingOut.wav";

            else if (soundType == SoundType.Disconnected)
                path = @"Resources/VoiceChatDisconnected.wav";

            else if (soundType == SoundType.MessageReceived)
                path = @"Resources/MessageReceived.wav";

            else if (soundType == SoundType.BuddyOnline)
                path = @"Resources/BuddyOnline.wav";

            player.SoundLocation = path;

            if (repeat)
                player.PlayLooping();
            else
                player.Play();
        }

        public void Stop()
        {
            player.Stop();
        }
    }
}
