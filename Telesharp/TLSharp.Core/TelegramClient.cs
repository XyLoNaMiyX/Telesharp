using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Telesharp;
using TLSharp.Core.Auth;
using TLSharp.Core.MTProto;
using TLSharp.Core.MTProto.Crypto;
using TLSharp.Core.Network;

namespace TLSharp.Core
{
    public static class Telegram
    {
        static MtProtoSender _sender;
        //static AuthKey _key;
        static TcpTransport _transport;
        static string _apiHash = "90d085d0be41e6b74c216132e072eced";
        static int _apiId = 19076;
        static Session _session;
        static List<DcOption> dcOptions;

        public static User loggedUser { get { return _session.User; } }

        public static List<Chat> chats;
        public static List<User> users;

        static string userTempHash;

        static string _Phone;
        public static string Phone {
            get { return _Phone; }
            set {
                if (!string.IsNullOrEmpty(value) && value[0] != '+')
                    _Phone = "+" + value.Replace(" ", "");
                else
                    _Phone = value.Replace(" ", "");
            }
        }

        static Telegram()
        {
            // TODO use settings
            string sessionUserId = "Lonami";

            var store = new FileSessionStore();
            _session = Session.TryLoadOrCreateNew(store, sessionUserId);
            _transport = new TcpTransport(_session.ServerAddress, _session.Port);
        }

        public static async Task<bool> Connect(bool reconnect = false)
        {
            try
            {
                if (_session.AuthKey == null || reconnect)
                {
                    var result = await Authenticator.DoAuthentication(_transport);
                    _session.AuthKey = result.AuthKey;
                    _session.TimeOffset = result.TimeOffset;
                }
                _sender = new MtProtoSender(_transport, _session);

                if (!reconnect)
                {
                    var request = new TL.InvokeWithLayerRequest(47,
                        new TL.InitConnectionRequest(_apiId, PCInfo.GetFullPCName(), PCInfo.GetOSName(), "0.9-BETA", "en",
                            new TL.HelpGetConfigRequest()));

                    await _sender.Send(request);
                    await _sender.Receive(request);

                    var result = (TL.ConfigType)request.Result;
                    dcOptions = result.DcOptions;
                }

                return true;
            }
            catch { return false; }
        }

        static async Task ReconnectToDc(int dcId)
        {
            if (dcOptions == null || !dcOptions.Any())
                throw new InvalidOperationException($"Can't reconnect. Establish initial connection first.");
            
            var dc = (TL.DcOptionType)dcOptions.First(d => ((TL.DcOptionType)d).Id == dcId);

            _transport = new TcpTransport(dc.IpAddress, dc.Port);
            _session.ServerAddress = dc.IpAddress;
            _session.Port = dc.Port;

            await Connect(true);
        }

        public static bool IsUserAuthorized()
        {
            return _session.User != null;
        }

        public static async Task<TL.AccountAuthorizationsType> GetAccountAuthorizations()
        {
            var request = new TL.AccountGetAuthorizationsRequest();

            await _sender.Send(request);
            await _sender.Receive(request);

            return (TL.AccountAuthorizationsType)request.Result;
        }

        public static async Task<bool> IsPhoneRegistered(string phoneNumber)
        {
            if (_sender == null)
                throw new InvalidOperationException("Not connected!");

            var authCheckPhoneRequest = new TL.AuthCheckPhoneRequest(phoneNumber);
            await _sender.Send(authCheckPhoneRequest);
            await _sender.Receive(authCheckPhoneRequest);

            var result = (TL.AuthCheckedPhoneType)authCheckPhoneRequest.Result;
            return result.PhoneRegistered;
        }

        public static async Task<bool> SendCodeRequest()
        {
            try
            {
                var completed = false;

                TL.AuthSendCodeRequest request = null;

                while (!completed)
                {
                    request = new TL.AuthSendCodeRequest(null, Phone, null, _apiId, _apiHash);
                    request = new TL.AuthSendCodeRequest(Phone, 5, _apiId, _apiHash, "en");
                    try
                    {
                        await _sender.Send(request);
                        await _sender.Receive(request);

                        completed = true;
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (ex.Message.StartsWith("Your phone number registered to") && ex.Data["dcId"] != null)
                        {
                            await ReconnectToDc((int)ex.Data["dcId"]);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                // TODO handle other types (such as SMS)
                var result = (TL.AuthSentCodeTypeAppType)request.Result;
                userTempHash = result.PhoneCodeHash;

                return true;
            }
            catch { return false; }
        }

        public static async Task<bool> MakeAuth(string code)
        {
            try
            {
                var request = new TL.AuthSignInRequest(Phone, userTempHash, code);
                await _sender.Send(request);
                await _sender.Receive(request);

                var result = (TL.AuthAuthorizationType)request.Result;
                _session.User = result.User;

                _session.Save();

                userTempHash = null;
                return true;
            }
            catch { return false; }
        }
        
        public static async Task<List<DialogCardInfo>> GetDialogs(int max)
        {
            var empty = new TL.InputPeerEmptyType();
            var request = new TL.MessagesGetDialogsRequest(0, 0, empty, max);
            await _sender.Send(request);
            await _sender.Receive(request);

            return DialogCardInfo.FromMessagesDialog(request.Result);
        }

        static bool gotOne;
        public static async Task<byte[]> DownloadFile(InputFileLocation ifl)
        {
            if (gotOne) return new byte[0];

            gotOne = true;
            if (ifl == null) return new byte[0];

            var bytes = new List<byte>();

            const int kilobyte = 1024;
            const int chunkSize = 512; // TODO try another

            for (int i = 0; ; i++)
            {
                var request = new TL.UploadGetFileRequest(
                    ifl, i * kilobyte * chunkSize, kilobyte * chunkSize);

                await _sender.Send(request); // TODO WHERE THE FUCK IS NULL
                await _sender.Receive(request);
                
                var result = (TL.UploadFileType)request.Result;
                if (result == null)
                    return new byte[0];
                
                bytes.AddRange(result.Bytes);

                if (result.Bytes.Length < kilobyte * chunkSize) break;
            }

            return bytes.ToArray();
        }

        public static async Task<BitmapImage> DownloadImage(InputFileLocation ifl)
        {
            return bytesToImage(await DownloadFile(ifl));
        }


        static BitmapImage bytesToImage(byte[] bytes)
        {
            if (bytes.Length == 0) return null;

            var image = new BitmapImage(); // could be a file stream
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit(); // load the image from the stream
            } // close the stream
            return image;
        }
    }
}
