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
using System.Windows.Input;
using System.ComponentModel;

namespace ChitChat
{
    enum FileTransferState
    {
        NotApplicable,
        Waiting,
        Transfering,
        Compleated
    }

    delegate void AcceptClickedHandler();
    delegate void OpenClickedHandler();

    class ChatMessage : INotifyPropertyChanged
    {
        public ChatMessage()
        {
            State = FileTransferState.NotApplicable;
        }

        public string From { get; set; }
        public string Text { get; set; }
        public string ArrivedAt { get; set; }
        public bool IsLocal { get; set; }

        FileTransferState _state;
        public FileTransferState State 
        {
            get { return _state; }
            set
            {
                _state = value;
                NotifyPropertyChanged("FileTransferStatus");
                NotifyPropertyChanged("AcceptLinkVisibility");
                NotifyPropertyChanged("TransferStatusVisibility");
                NotifyPropertyChanged("OpenFileLinkVisibility");
            }
        }

        public string FileTransferStatus 
        {
            get
            {
                string retVal = string.Empty;

                if (IsLocal)
                {
                    if (State == FileTransferState.Waiting)
                        retVal = "Waiting...";
                    else if (State == FileTransferState.Transfering)
                        retVal = "Sending File...";
                    else if (State == FileTransferState.Compleated)
                        retVal = "Done";
                }
                else
                {
                    if (State == FileTransferState.Transfering)
                        retVal = "Downloading...";
                }

                return retVal;
            }
        }

        #region ReadOnly Value Converter Properties

        public string SourceNameColor
        {
            get
            {
                return IsLocal ? "Gray" : "Blue";
            }
        }

        public string FileIconVisibility
        {
            get
            {
                return (State == FileTransferState.NotApplicable) ? "Collapsed" : "Visible";
            }
        }

        public string AcceptLinkVisibility
        {
            get
            {
                return (State == FileTransferState.Waiting && !IsLocal) ? "Visible" : "Collapsed";
            }
        }

        public string TransferStatusVisibility
        {
            get
            {
                return (!IsLocal && State == FileTransferState.Transfering) ||
                       (IsLocal && State != FileTransferState.NotApplicable) ? "Visible" : "Collapsed";
            }
        }

        public string OpenFileLinkVisibility
        {
            get
            {
                return (State == FileTransferState.Compleated && !IsLocal) ? "Visible" : "Collapsed";
            }
        }

        #endregion

        #region Commands

        public ICommand AcceptCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    OnAcceptClicked();
                },
                delegate { return true; });
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    OnOpenClicked();
                },
                delegate { return true; });
            }
        }

        #endregion

        #region Events

        public event AcceptClickedHandler AcceptClicked;
        void OnAcceptClicked()
        {
            if (AcceptClicked != null)
                AcceptClicked();
        }

        public event OpenClickedHandler OpenClicked;
        void OnOpenClicked()
        {
            if (OpenClicked != null)
                OpenClicked();
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
