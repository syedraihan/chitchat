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
using System.Text;

namespace ChitChat
{
    delegate void PeerAddedHandler(string hostName, string ipAddress);
    delegate void PeerRemovedHandler(string hostName);
    delegate void IncomingCallHandler(string hostName);
    delegate void CallAcceptedHandler();
    delegate void EndCallRequestedHandler();
    delegate void TextMessageArrivedHandler(string hostName, string message);
    delegate void FileSendRequestedHandler(string hostName, string fileName);
    delegate void FileAcceptedHandler(string hostName, string fileName);

    class P2PNetwork
    {
        UdpReceiver receiver;
        UdpSender sender;
        NetworkInfo networkInfo;

        public P2PNetwork()
        {
            networkInfo = new NetworkInfo();

            sender = new UdpSender(networkInfo.BroadcastIP, 11001);
            receiver = new UdpReceiver(11001);
            receiver.DataArrived += delegate(byte[] data, string senderIP)
            {
                string message = Encoding.ASCII.GetString(data);
                ProcessMessage(message, senderIP);
            };
        }

        public void Connent()
        {
            receiver.Start();
            SendMessage("HELLO", "ALL");
        }

        public void Close()
        {
            SendMessage("BYE", "ALL");
            receiver.Close();
        }

        public void CallHost(string hostName)
        {
            SendMessage("RING", hostName);
        }

        public void AcceptCall(string hostName)
        {
            SendMessage("CALL_ACCEPTED", hostName);
        }

        public void EndCall(string hostName)
        {
            SendMessage("END_CALL", hostName);
        }

        public void SendText(string hostName, string message)
        {
            SendMessageWithParam("TEXT", hostName, message);
        }

        public void SendFile(string hostName, string fileName)
        {
            SendMessageWithParam("FILE", hostName, fileName);
        }

        public void AcceptFile(string hostName, string fileName)
        {
            SendMessageWithParam("FILE_ACCEPTED", hostName, fileName);
        }

        void SendMessage(string msg, string targetHostName)
        {
            SendMessageWithParam(msg, targetHostName, string.Empty);
        }

        void SendMessageWithParam(string msg, string targetHostName, string param)
        {
            string message = string.Format("{0}:{1}:{2}:{3}", msg, targetHostName, networkInfo.HostName, param);

            byte[] data = Encoding.ASCII.GetBytes(message);
            sender.Send(data);
        }

        void ProcessMessage(string message, string senderIP)
        {
            if (senderIP.Equals(networkInfo.HostIP))
                return; // Ignore your own boradcast

            string[] parts = message.Split(":".ToCharArray());
            if (parts.Length != 4)
                return; // Bad message format

            string msg = parts[0];
            string target = parts[1];
            string senderHostName = parts[2];
            string param = parts[3];

            if (!(target.Equals("ALL") || target.Equals(networkInfo.HostName)))
                return; // Not for me

            if (msg.Equals("HELLO"))
            {
                OnPeerAdded(senderHostName, senderIP);
                SendMessage("WELCOME", senderHostName);
            }
            if (msg.Equals("WELCOME"))
            {
                OnPeerAdded(senderHostName, senderIP);
            }
            else if (msg.Equals("BYE"))
            {
                OnPeerRemoved(senderHostName);
            }
            else if (msg.Equals("RING"))
            {
                OnIncomingCall(senderHostName);
            }
            else if (msg.Equals("CALL_ACCEPTED"))
            {
                OnCallAccepted();
            }
            else if (msg.Equals("END_CALL"))
            {
                OnEndCall();
            }
            else if (msg.Equals("TEXT"))
            {
                OnTextMessageArrived(senderHostName, param);
            }
            else if (msg.Equals("FILE"))
            {
                OnFileSendRequested(senderHostName, param);
            }
            else if (msg.Equals("FILE_ACCEPTED"))
            {
                OnFileAccepted(senderHostName, param);
            }
        }

        #region Events

        public event PeerAddedHandler PeerAdded;
        void OnPeerAdded(string hostName, string ipAddress)
        {
            if (PeerAdded != null)
            {
                PeerAdded(hostName, ipAddress);
            }
        }

        public event PeerRemovedHandler PeerRemoved;
        void OnPeerRemoved(string hostName)
        {
            if (PeerRemoved != null)
                PeerRemoved(hostName);
        }

        public event IncomingCallHandler IncomingCall;
        void OnIncomingCall(string hostName)
        {
            if (IncomingCall != null)
                IncomingCall(hostName);
        }

        public event CallAcceptedHandler CallAccepted;
        void OnCallAccepted()
        {
            if (CallAccepted != null)
                CallAccepted();
        }

        public event EndCallRequestedHandler EndCallRequested;
        void OnEndCall()
        {
            if (EndCallRequested != null)
                EndCallRequested();
        }

        public event TextMessageArrivedHandler TextMessageArrived;
        void OnTextMessageArrived(string hostName, string message)
        {
            if (TextMessageArrived != null)
                TextMessageArrived(hostName, message);
        }

        public event FileSendRequestedHandler FileSendRequested;
        void OnFileSendRequested(string hostName, string fileName)
        {
            if (FileSendRequested != null)
                FileSendRequested(hostName, fileName);
        }

        public event FileAcceptedHandler FileAccepted;
        void OnFileAccepted(string hostName, string fileName)
        {
            if (FileAccepted != null)
                FileAccepted(hostName, fileName);
        }

        #endregion
    }
}
