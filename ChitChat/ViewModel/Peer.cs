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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChitChat
{
    enum CallStates
    {
        Idle,
        RingIn,
        RingOut,
        CallInProgress
    }

    class Peer : INotifyPropertyChanged
    {
        ObservableCollection<ChatMessage> _messages;

        public Peer()
        {
            _messages = new ObservableCollection<ChatMessage>();
            State = CallStates.Idle;
        }

        public string HostName { get; set; }
        public string IPAddress { get; set; }

        public ObservableCollection<ChatMessage> Messages { get { return _messages; } }

        public ChatMessage GetMessage(string fileName)
        {
            var query = from m in _messages where m.Text == fileName select m;
            return query.First<ChatMessage>();
        }

        #region Read/Write Properties

        string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyPropertyChanged("Message");
            }
        }

        int _unreadMessageCount;
        public int UnreadMessageCount
        {
            get { return _unreadMessageCount; }
            set
            {
                _unreadMessageCount = value;
                NotifyPropertyChanged("Text");
                NotifyPropertyChanged("IsBold");
            }
        }

        bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (value)
                    UnreadMessageCount = 0;

                NotifyPropertyChanged("IsSelected");
            }
        }

        CallStates _state;
        public CallStates State 
        {
            get { return _state; } 
            set
            {
                _state = value;
                NotifyPropertyChanged("CallStateDescription");
                NotifyPropertyChanged("GreenButtonVisibility");
                NotifyPropertyChanged("GreenButtonText");
                NotifyPropertyChanged("RedButtonVisibility");
                NotifyPropertyChanged("RedButtonText");
                NotifyPropertyChanged("Icon");
            } 
        }

        #endregion

        #region ReadOnly Properties

        public string Text
        {
            get
            {
                string extraInfo = string.Empty;

                if (this.State == CallStates.CallInProgress)
                    extraInfo = " 00:00";
                else
                    extraInfo = UnreadMessageCount == 0 ? string.Empty : string.Format(" ({0})", UnreadMessageCount);

                return string.Format("{0}{1}", HostName, extraInfo);
            }
        }

        public string CallStateDescription
        {
            get
            {
                string retVal = string.Empty;

                if (this.State == CallStates.RingOut)
                    retVal = string.Format("Dialing {0}...", HostName);

                else if (this.State == CallStates.RingIn)
                    retVal = string.Format("Incoming Call from {0}...", HostName);

                else if (this.State == CallStates.CallInProgress)
                    retVal = string.Format("Talking with {0}...", HostName);

                return retVal;
            }
        }

        public string Icon
        {
            get
            {
                string retVal = "/ChitChat;component/Resources/";

                if (State == CallStates.Idle)
                    return retVal + "computer.png";
                else
                    return retVal + "phone.png";
            }
        }

        #endregion

        #region ReadOnly Value Converter Properties

        public string GreenButtonText
        {
            get
            {
                return this.State == CallStates.RingIn ? "Accept" : "Call";
            }
        }

        public string GreenButtonVisibility
        {
            get
            {
                return (this.State == CallStates.Idle ||
                        this.State == CallStates.RingIn) ? "Visible" : "Collapsed";
            }
        }

        public string RedButtonText
        {
            get
            {
                return this.State == CallStates.RingIn ? "Reject" : "Hangup";
            }
        }

        public string RedButtonVisibility
        {
            get
            {
                return (this.State == CallStates.Idle) ? "Collapsed" : "Visible";
            }
        }

        public string IsBold
        {
            get
            {
                return UnreadMessageCount == 0 ? "Normal" : "Bold";
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
