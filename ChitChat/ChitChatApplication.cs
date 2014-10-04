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
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace ChitChat
{
    class ChitChatApplication
    {
        ChitChatViewModel view;
        P2PNetwork p2pNetwork;

        SoundManager soundManager;
        VoiceNetwork voiceNetwork;
        VoiceChat voiceChat;

        Peer peer;

        Dictionary<string, string> fileTransferRequests;
        FileTransfer fileTransfer;

        public ChitChatApplication(ChitChatViewModel view)
        {
            this.view = view;
            SetupView();

            p2pNetwork = new P2PNetwork();
            SetupP2PNetwork();
            p2pNetwork.Connent();

            soundManager = new SoundManager();
            fileTransferRequests = new Dictionary<string, string>();

            if (!Directory.Exists(@"Downloads"))
                Directory.CreateDirectory(@"Downloads");
        }

        void SetupView()
        {
            view.CallPeer += delegate(string hostName)
            {
                peer = view.GetPeer(hostName);

                soundManager.Play(SoundType.RingOut, true);
                p2pNetwork.CallHost(hostName);
            };

            view.EndCall += delegate()
            {
                p2pNetwork.EndCall(peer.HostName);
                EndCall();
            };

            view.AcceptCall += delegate()
            {
                p2pNetwork.AcceptCall(peer.HostName);
                StartCall(peer.IPAddress);
            };

            view.SendTextRequested += delegate(string hostName, string message)
            {
                p2pNetwork.SendText(hostName, message);
            };

            view.SendFileRequested += delegate(string hostName, string filePath)
            {
                string fileName = Path.GetFileName(filePath);
                fileTransferRequests.Add(fileName, filePath);

                p2pNetwork.SendFile(hostName, fileName);
            };

            view.FileAccpted += delegate(string hostName, string fileName)
            {
                var peer = view.GetPeer(hostName);
                var msg = peer.GetMessage(fileName);
                msg.State = FileTransferState.Transfering;

                fileTransfer = new FileTransfer();
                fileTransfer.ReceiveCompleted += delegate()
                {
                    msg.State = FileTransferState.Compleated;
                };
                fileTransfer.ReceiveFile(fileName);

                p2pNetwork.AcceptFile(hostName, fileName);
            };

            view.OpenFile += delegate(string fileName)
            {
                var psi = new ProcessStartInfo(@"Downloads\" + fileName);
                psi.UseShellExecute = true;
                Process.Start(psi);
            };
        }

        void SetupP2PNetwork()
        {
            p2pNetwork.PeerAdded += delegate(string hostName, string ipAddress)
            {
                view.AddPeer(hostName, ipAddress);
                soundManager.Play(SoundType.BuddyOnline, false);
            };

            p2pNetwork.PeerRemoved += delegate(string hostName)
            {
                view.RemovePeer(hostName);
            };

            p2pNetwork.IncomingCall += delegate(string hostName)
            {
                peer = view.GetPeer(hostName);

                if (peer.State == CallStates.Idle)
                {
                    view.SelectedPeer.IsSelected = false;
                    peer.IsSelected = true;
                    peer.State = CallStates.RingIn;
                    soundManager.Play(SoundType.RingIn, true);
                }
            };

            p2pNetwork.CallAccepted += delegate()
            {
                peer.State = CallStates.CallInProgress;
                StartCall(peer.IPAddress);
            };

            p2pNetwork.EndCallRequested += delegate()
            {
                peer.State = CallStates.Idle;
                EndCall();
            };

            p2pNetwork.TextMessageArrived += delegate(string hostName, string message)
            {
                view.AddNewMessage(hostName, message, false);
                soundManager.Play(SoundType.MessageReceived, false);
            };

            p2pNetwork.FileSendRequested += delegate(string hostName, string fileName)
            {
                view.AddNewMessage(hostName, fileName, true);
                soundManager.Play(SoundType.MessageReceived, false);
            };

            p2pNetwork.FileAccepted += delegate(string hostName, string fileName)
            {
                var peer = view.GetPeer(hostName);
                string filePath = fileTransferRequests[fileName];

                fileTransfer = new FileTransfer();
                fileTransfer.SendFile(filePath, peer.IPAddress);
                fileTransferRequests.Remove(fileName);

                var msg = peer.GetMessage(fileName);
                msg.State = FileTransferState.Compleated;
            };
        }

        void StartCall(string targetIP)
        {
            soundManager.Stop();

            voiceChat = new VoiceChat();
            voiceNetwork = new VoiceNetwork(targetIP);
            voiceNetwork.IncomeVoiceAvailable += delegate(byte[] data)
            {
                voiceChat.Play(data);
            };
            voiceChat.OutgoingVoiceAvailable += delegate(byte[] data)
            {
                voiceNetwork.Send(data);
            };
            voiceChat.Start();
            voiceNetwork.Start();
        }

        void EndCall()
        {
            soundManager.Stop();

            if (voiceChat != null)
                voiceChat.Stop();

            if (voiceNetwork != null)
                voiceNetwork.Stop();
        }

        public void Close()
        {
            EndCall();
            p2pNetwork.Close();
        }
    }
}
