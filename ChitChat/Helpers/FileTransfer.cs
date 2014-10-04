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
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace ChitChat
{
    delegate void ReceiveCompletedHandler();

    class FileTransfer
    {
        Dispatcher _dispatcher;

        public FileTransfer()
        {
            _dispatcher = App.Current.Dispatcher;
        }

        public void SendFile(string filePath, string targetIP)
        {
            var fs = File.OpenRead(filePath);
            var buffer = new Byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);

            var socket = new TcpClient(targetIP, 11002);
            var nw = socket.GetStream();
            nw.Write(buffer, 0, buffer.Length);
            nw.Close();
        }

        public void ReceiveFile(string fileName)
        {
            ThreadPool.QueueUserWorkItem(this.ListenerThread, @"Downloads\" + fileName);
        }

        void ListenerThread(object state)
        {
            string filePath = (string)state;

            try
            {
                var listener = new TcpListener(11002);
                listener.Start();

                var socket = listener.AcceptTcpClient();
                var nw = new NetworkStream(socket.Client);

                int thisRead = 0;
                int blockSize = 1024;
                var buffer = new Byte[blockSize];

                var fs = File.OpenWrite(filePath);
                while (true)
                {
                    thisRead = nw.Read(buffer, 0, blockSize);
                    fs.Write(buffer, 0, thisRead);
                    if (thisRead == 0)
                        break;
                }
                fs.Close();
                socket.Close();

                OnReceiveCompleted();
            }
            catch (SocketException)
            {
                // usually not a problem - just means we have disconnected
            }
        }

        public event ReceiveCompletedHandler ReceiveCompleted;
        void OnReceiveCompleted()
        {
            if (ReceiveCompleted != null)
            {
                _dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ReceiveCompleted();
                }));
            }
        }
    }
}
