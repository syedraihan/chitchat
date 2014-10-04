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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace ChitChat
{
    delegate void DataArrivedHandler(byte[] data, string senderIP);

    class UdpReceiver
    {
        UdpClient sock;
        IPEndPoint endPoint;

        public event DataArrivedHandler DataArrived;

        bool connected = false;
        Dispatcher _dispatcher;

        public UdpReceiver(int port) : this(string.Empty, port)
        {

        }

        public UdpReceiver(string address, int port) 
        {
            _dispatcher = App.Current.Dispatcher;

            IPAddress ipAddress;
            if (address.Length == 0) 
                ipAddress = IPAddress.Any;
            else
                ipAddress = IPAddress.Parse(address);

            endPoint = new IPEndPoint(ipAddress, port);
            sock = new UdpClient(port);
        }

        public void Start()
        {
            connected = true;
            ListenerThreadState state = new ListenerThreadState() { EndPoint = endPoint };
            ThreadPool.QueueUserWorkItem(this.ListenerThread, state);
        }

        public void Close()
        {
            connected = false;
            sock.Close();
        }

        void ListenerThread(object state)
        {
            var listenerThreadState = (ListenerThreadState)state;
            var endPoint = listenerThreadState.EndPoint;
            try
            {
                while (connected)
                {
                    byte[] data = this.sock.Receive(ref endPoint);
                    OnDataArrived(data, endPoint.Address.ToString());
                }
            }
            catch (SocketException)
            {
                // usually not a problem - just means we have disconnected
            }
        }

        void OnDataArrived(byte[] data, string senderIP)
        {
            if (DataArrived != null)
            {
                _dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => 
                { 
                    DataArrived(data, senderIP); 
                }));
            }
        }
    }

    class ListenerThreadState
    {
        public IPEndPoint EndPoint { get; set; }
    }
}
