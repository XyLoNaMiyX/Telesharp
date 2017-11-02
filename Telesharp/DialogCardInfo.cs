using System.Collections.Generic;
using TLSharp.Core.MTProto;

namespace Telesharp
{
    public class DialogCardInfo
    {
        public string _TopMessage;
        public string TopMessage
        {
            get { return _TopMessage; }
            set { _TopMessage = value.Replace('\n', ' '); }
        }
        public MessageType TopMessageType;
        public enum MessageType { Empty, Normal, Service }

        public string DialogName;
        public DialogType DialogNameType;
        public enum DialogType { User, Chat, Channel }
        
        public long? PhotoID; // only has value for users
        public FileLocation PhotoSmall;
        public FileLocation PhotoBig;

        public bool IsEmpty;
        public bool IsForbidden;

        DialogCardInfo() { }

        // TODO save more stuff of everything! there's a lot of unused information such as topimportantmessage in dialogchannel etc
        public static List<DialogCardInfo> FromMessagesDialog(MessagesDialogs messagesDialogs)
        {
            var dialogCards = new List<DialogCardInfo>();
            
            if (messagesDialogs is TL.MessagesDialogsSliceType)
            {
                var dialogs = (TL.MessagesDialogsSliceType)messagesDialogs;
                foreach (var dialog in dialogs.Dialogs)
                {
                    var dcard = new DialogCardInfo();
                    checkDialog(dcard, dialog, dialogs.Messages, dialogs.Users, dialogs.Chats);
                    dialogCards.Add(dcard);
                }
            }
            else if (messagesDialogs is TL.MessagesDialogsType)
            {
                var dialogs = (TL.MessagesDialogsType)messagesDialogs;
                foreach (var dialog in dialogs.Dialogs)
                {
                    var dcard = new DialogCardInfo();
                    checkDialog(dcard, dialog, dialogs.Messages, dialogs.Users, dialogs.Chats);
                    dialogCards.Add(dcard);
                }
            }

            return dialogCards;
        }

        static void checkDialog(DialogCardInfo dcard, Dialog dialog, List<Message> messages, List<User> users, List<Chat> chats)
        {
            if (dialog is TL.DialogType)
            {
                var dt = (TL.DialogType)dialog;

                checkTopMessage(dcard, dt.TopMessage, messages);
                checkPeer(dcard, dt.Peer, users, chats);
            }
            else if (dialog is TL.DialogChannelType)
            {
                var dct = (TL.DialogChannelType)dialog;
                
                checkTopMessage(dcard, dct.TopMessage, messages);
                checkPeer(dcard, dct.Peer, users, chats);
            }
        }

        static void checkTopMessage(DialogCardInfo dcard, int topMessageId, List<Message> messages)
        {
            // look for top message
            foreach (var message in messages)
            {
                if (message is TL.MessageEmptyType)
                {
                    var met = (TL.MessageEmptyType)message;
                    if (met.Id == topMessageId)
                    {
                        dcard.TopMessage = "(empty)";
                        dcard.TopMessageType = MessageType.Empty;
                        break;
                    }
                }
                else if (message is TL.MessageType)
                {
                    var mt = (TL.MessageType)message;
                    if (mt.Id == topMessageId)
                    {
                        dcard.TopMessage = mt.Message;
                        dcard.TopMessageType = MessageType.Normal;
                        break;
                    }
                }
                else if (message is TL.MessageServiceType)
                {
                    var mst = (TL.MessageType)message;
                    if (mst.Id == topMessageId)
                    {
                        dcard.TopMessage = mst.Message;
                        dcard.TopMessageType = MessageType.Service;
                        break;
                    }
                }
            }
        }

        static void checkPeer(DialogCardInfo dcard, Peer peer, List<User> users, List<Chat> chats)
        {
            if (peer is TL.PeerUserType)
            {
                dcard.DialogNameType = DialogType.User;
                var put = (TL.PeerUserType)peer;

                foreach (var user in users)
                {
                    if (user is TL.UserEmptyType)
                    {
                        var uet = (TL.UserEmptyType)user;
                        if (uet.Id == put.UserId)
                        {
                            dcard.DialogName = "(empty)";
                            dcard.IsEmpty = true;
                        }
                    }
                    else if (user is TL.UserType)
                    {
                        var ut = (TL.UserType)user;
                        if (ut.Id == put.UserId)
                        {
                            dcard.DialogName = ut.FirstName + " " + ut.LastName;
                            checkPhoto(dcard, ut.Photo);
                        }
                    }
                }
            }
            else if (peer is TL.PeerChatType)
            {
                dcard.DialogNameType = DialogType.Chat;
                var pct = (TL.PeerChatType)peer;

                foreach (var chat in chats)
                {
                    if (chat is TL.ChatEmptyType)
                    {
                        var cet = (TL.ChatEmptyType)chat;
                        if (cet.Id == pct.ChatId)
                        {
                            dcard.DialogName = "(empty)";
                            dcard.IsEmpty = true;
                        }
                    }
                    else if (chat is TL.ChatType)
                    {
                        var ct = (TL.ChatType)chat;
                        if (ct.Id == pct.ChatId)
                        {
                            dcard.DialogName = ct.Title;
                            checkPhoto(dcard, ct.Photo);
                        }
                    }
                    else if (chat is TL.ChatForbiddenType)
                    {
                        var cbt = (TL.ChatForbiddenType)chat;
                        if (cbt.Id == pct.ChatId)
                        {
                            dcard.DialogName = cbt.Title;
                            dcard.IsForbidden = true;
                        }
                    }
                }
            }
            else if (peer is TL.PeerChannelType)
            {
                dcard.DialogNameType = DialogType.Channel;
                var pct = (TL.PeerChannelType)peer;

                foreach (var chat in chats)
                {
                    if (chat is TL.ChannelType)
                    {
                        var ct = (TL.ChannelType)chat;
                        if (ct.Id == pct.ChannelId)
                        {
                            dcard.DialogName = ct.Title;
                            checkPhoto(dcard, ct.Photo);
                        }
                    }
                    else if (chat is TL.ChannelForbiddenType)
                    {
                        var cft = (TL.ChannelForbiddenType)chat;
                        if (cft.Id == pct.ChannelId)
                        {
                            dcard.DialogName = cft.Title;
                            dcard.IsForbidden = true;
                        }
                    }
                }
            }
        }

        static void checkPhoto(DialogCardInfo dcard, UserProfilePhoto photo)
        {
            if (photo is TL.UserProfilePhotoType)
            {
                var uppt = (TL.UserProfilePhotoType)photo;
                dcard.PhotoID = uppt.PhotoId;
                dcard.PhotoSmall = uppt.PhotoSmall;
                dcard.PhotoBig = uppt.PhotoBig;
            }
            // else (ut.Photo is TL.UserProfilePhotoEmptyType)
        }

        static void checkPhoto(DialogCardInfo dcard, ChatPhoto photo)
        {
            if (photo is TL.ChatPhotoType)
            {
                var cpt = (TL.ChatPhotoType)photo;
                dcard.PhotoSmall = cpt.PhotoSmall;
                dcard.PhotoBig = cpt.PhotoBig;
            }
            // else (ut.Photo is TL.ChatPhotoEmptyType)
        }
    }
}
