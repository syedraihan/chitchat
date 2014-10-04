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
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;

namespace ChitChat
{
    delegate void SendTextRequestedHandler(string hostName, string message);
    delegate void CallPeerHandler(string hostName);
    delegate void AcceptCallHandler();
    delegate void EndCallHandler();
    delegate void SendFileRequestedHandler(string hostName, string filePath);
    delegate void FileAccptedHandler(string hostName, string fileName);
    delegate void OpenFileHandler(string fileName);

    class ChitChatViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Peer> _peers;
        Peer _selectedPeer;

        public ChitChatViewModel()
        {
            _peers = new ObservableCollection<Peer>();
        }

        #region Public Properties

        public ObservableCollection<Peer> Peers { get { return _peers; } }

        public Peer SelectedPeer
        {
            get { return _selectedPeer; }
            set
            {
                _selectedPeer = value;
                NotifyPropertyChanged("SelectedPeer");
                NotifyPropertyChanged("ChatPanelVisibility");
            }
        }

        #endregion

        #region Public Methods

        public void AddPeer(string peerName, string ipAddress)
        {
            var peer = new Peer() { HostName = peerName, IPAddress = ipAddress};
            peer.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("IsSelected"))
                    this.SelectedPeer = (Peer)sender;
            };

            _peers.Add(peer);

            if (SelectedPeer == null)
                peer.IsSelected = true;
        }

        public void RemovePeer(string peerName)
        {
            var peer = GetPeer(peerName);
            _peers.Remove(peer);

            if (_peers.Count == 0)
            {
                SelectedPeer = null;
                NotifyPropertyChanged("ChatPanelVisibility");
            }
        }

        public Peer GetPeer(string peerName)
        {
            var query = from p in _peers where p.HostName == peerName select p;
            return query.First<Peer>();
        }

        public void AddNewMessage(string peerName, string message, bool isFile)
        {
            var peer = GetPeer(peerName);

            string fileName = message;

            var msg = new ChatMessage()
            {
                From = peerName,
                Text = fileName,
                State = isFile ? FileTransferState.Waiting : FileTransferState.NotApplicable,
                ArrivedAt = DateTime.Now.ToShortTimeString()
            };

            msg.AcceptClicked += delegate() { OnFileAccpted(peerName, fileName); };
            msg.OpenClicked += delegate() { OnOpenFile(fileName); };

            peer.Messages.Add(msg);

            if (peer != this.SelectedPeer)
                peer.UnreadMessageCount = peer.UnreadMessageCount + 1;
        }

        #endregion

        #region Commands

        public ICommand SendTextCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    var peer = SelectedPeer;

                    OnSendTextRequested(peer.HostName, peer.Message);

                    peer.Messages.Add(new ChatMessage()
                    {
                        From = "You",
                        IsLocal = true,
                        Text = peer.Message,
                        ArrivedAt = DateTime.Now.ToShortTimeString()
                    });
                    peer.Message = string.Empty;
                },
                delegate { return true; });
            }
        }

        public ICommand GreenClickCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    var peer = SelectedPeer;

                    if (peer.State == CallStates.Idle)          // Dial out
                    {
                        peer.State = CallStates.RingOut;
                        OnCallPeer(peer.HostName);
                    }
                    else if (peer.State == CallStates.RingIn)   // Pick up
                    {
                        peer.State = CallStates.CallInProgress;
                        OnAcceptCall();
                    }
                },
                delegate { return true; });
            }
        }

        public ICommand RedClickCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    SelectedPeer.State = CallStates.Idle;
                    OnEndCall();
                },
                delegate { return true; });
            }
        }

        public ICommand SendFileCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    var peer = SelectedPeer;

                    var dlg = new OpenFileDialog();
                    if (dlg.ShowDialog() == false)
                        return;

                    peer.Messages.Add(new ChatMessage()
                    {
                        From = "You",
                        IsLocal = true,
                        Text = Path.GetFileName(dlg.FileName),
                        State = FileTransferState.Waiting, 
                        ArrivedAt = DateTime.Now.ToShortTimeString()
                    });

                    OnSendFileRequested(peer.HostName, dlg.FileName);
                },
                delegate { return true; });
            }
        }

        #endregion

        #region Events

        public event SendTextRequestedHandler SendTextRequested;
        void OnSendTextRequested(string hostName, string message)
        {
            if (SendTextRequested != null)
                SendTextRequested(hostName, message);
        }

        public event CallPeerHandler CallPeer;
        void OnCallPeer(string hostName)
        {
            if (CallPeer != null)
                CallPeer(hostName);
        }

        public event AcceptCallHandler AcceptCall;
        void OnAcceptCall()
        {
            if (AcceptCall != null)
                AcceptCall();
        }

        public event EndCallHandler EndCall;
        void OnEndCall()
        {
            if (EndCall != null)
                EndCall();
        }

        public event SendFileRequestedHandler SendFileRequested;
        void OnSendFileRequested(string hostName, string filePath)
        {
            if (SendFileRequested != null)
                SendFileRequested(hostName, filePath);
        }

        public event FileAccptedHandler FileAccpted;
        void OnFileAccpted(string hostName, string fileName)
        {
            if (FileAccpted != null)
                FileAccpted(hostName, fileName);
        }

        public event OpenFileHandler OpenFile;
        void OnOpenFile(string fileName)
        {
            if (OpenFile != null)
                OpenFile(fileName);
        }

        #endregion

        #region ReadOnly Value Converter Properties

        public string ChatPanelVisibility
        {
            get
            {
                return SelectedPeer == null ? "Collapsed" : "Visible";
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
