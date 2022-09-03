using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace UDP_Asynchronous_Chat
{
    public class ChatClient : UDPChatGeneral
    {
        Socket mSockBroadcastSender;
        IPEndPoint mIPEPBroadcast;
        IPEndPoint mIPEPLocal;
        EndPoint mChatServerEP;

        public ChatClient(int _localport, int _remoteport)
        {
            mIPEPBroadcast = new IPEndPoint(IPAddress.Broadcast, _remoteport);
            mIPEPLocal= new IPEndPoint(IPAddress.Any, _localport);
            mSockBroadcastSender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mSockBroadcastSender.EnableBroadcast = true;
        }

        public void SendBroadcast(string strDataForBroadcast)
        {
            if (string.IsNullOrEmpty(strDataForBroadcast))
            {
                return;
            }

            try
            {
                if (!mSockBroadcastSender.IsBound)
                {
                    mSockBroadcastSender.Bind(mIPEPLocal);
                }
                ChatPacket objChatPacket = new ChatPacket();
                objChatPacket.Message = strDataForBroadcast;
                objChatPacket.PacketType = PACKET_TYPE.DISCOVERY;
                string strJSONDiscovery = JsonConvert.SerializeObject(objChatPacket);

                var dataBytes = Encoding.ASCII.GetBytes(strJSONDiscovery);

                SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                saea.SetBuffer(dataBytes, 0 , dataBytes.Length);
                saea.RemoteEndPoint = mIPEPBroadcast;
                saea.UserToken = objChatPacket;

                saea.Completed += SendCompletedCallback;
                mSockBroadcastSender.SendToAsync(saea);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        } 

        private void SendCompletedCallback(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine($"Data send succesfully to: {e.RemoteEndPoint}");
            if((e.UserToken as ChatPacket).PacketType== PACKET_TYPE.DISCOVERY)
                ReceiveTextFromServer(expectedValue: "<CONFIRM>", IPEPReceiverLocal: mIPEPLocal);
        }

        private void ReceiveTextFromServer(string expectedValue, IPEndPoint IPEPReceiverLocal)
        {
            if (IPEPReceiverLocal == null)
            {
                Console.WriteLine("No IP Endpoint specified.");
                return;
            }

            SocketAsyncEventArgs saeaSendConfirmation = new SocketAsyncEventArgs();
            saeaSendConfirmation.SetBuffer(new byte[64000],0,64000);
            saeaSendConfirmation.RemoteEndPoint = IPEPReceiverLocal;
            saeaSendConfirmation.UserToken = expectedValue;
            saeaSendConfirmation.Completed += receivedContent;
            mSockBroadcastSender.ReceiveFromAsync(saeaSendConfirmation);

        }

        public void SendImage(string fileName, byte[] file, string Message)
        {
            ChatPacket packImage= new ChatPacket();
            packImage.PacketType = PACKET_TYPE.IMAGE;
            packImage.RawData = file;
            packImage.Message = Message;
            packImage.FileInfo = fileName;
            var bytesToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(packImage));
            SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
            saea.SetBuffer(bytesToSend,0,bytesToSend.Length);
            saea.UserToken = fileName;
            saea.Completed += SendMessageToKnownServerCompletedCallback;
            var retVal = mSockBroadcastSender.SendToAsync(saea);
            OnRaisePrintStringEvent(new PrintStringEventArgs($"Image transfer status, returned: {retVal} - socket error: {saea.SocketError}"));
            
        }

        private void receivedContent(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                Debug.WriteLine($"Zero Bytes transferred, socket error: {e.SocketError}");
                return;
            }

            var receivedText = Encoding.ASCII.GetString(e.Buffer,0,e.BytesTransferred);
            ChatPacket objPacketConfirmation = JsonConvert.DeserializeObject<ChatPacket>(receivedText);

            if (objPacketConfirmation.PacketType== PACKET_TYPE.CONFIRMATION
                && receivedText.Equals(Convert.ToString(e.UserToken)))
            {
                Console.WriteLine($"Received confirmation from server. {e.RemoteEndPoint}");
                OnRaisePrintStringEvent(
                    new PrintStringEventArgs($"Received confirmation from server. {e.RemoteEndPoint}"));
                mChatServerEP = e.RemoteEndPoint;
                ReceiveTextFromServer(String.Empty,mChatServerEP as IPEndPoint);
            }
            else if(objPacketConfirmation.PacketType == PACKET_TYPE.TEXT &&
                    !string.IsNullOrEmpty(objPacketConfirmation.Message))
            {
                Console.WriteLine($"Text Received: {objPacketConfirmation.Message}");
                OnRaisePrintStringEvent(new PrintStringEventArgs($"Text Received: {receivedText}"));

                ReceiveTextFromServer(string.Empty, mChatServerEP as IPEndPoint);
            }else if (objPacketConfirmation.PacketType == PACKET_TYPE.IMAGE)
            {
                OnRaiseImageReceived(new ImageReceived(
                    fileName: objPacketConfirmation.FileInfo,
                    fileData: objPacketConfirmation.RawData,
                    message: objPacketConfirmation.Message));
                ReceiveTextFromServer(string.Empty,mChatServerEP as IPEndPoint);
            }
            else if (objPacketConfirmation.PacketType == PACKET_TYPE.CONFIRMATION &&
                     !string.IsNullOrEmpty(Convert.ToString(e.UserToken))
                     && !receivedText.Equals(Convert.ToString(e.UserToken)))
            {
                Console.WriteLine($"Expected token not returned by the server");
                OnRaisePrintStringEvent(new PrintStringEventArgs($"Expected token not returned by the server"));
            }
        }

        public EventHandler<ImageReceived> IR;
        private void OnRaiseImageReceived(ImageReceived imageReceived)
        {
            EventHandler<ImageReceived> handler = IR;
            if (IR != null)
            {
                IR(this, imageReceived);
            }


        }

        public void SendMessageToKnownServer(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    return;
                ChatPacket objMessagePacket = new ChatPacket();
                objMessagePacket.Message = message;
                objMessagePacket.PacketType = PACKET_TYPE.TEXT;

                var bytesToSend = Encoding.ASCII.GetBytes(
                    JsonConvert.SerializeObject(objMessagePacket));

                SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                saea.SetBuffer(bytesToSend, 0, bytesToSend.Length);
                saea.RemoteEndPoint = mChatServerEP;
                saea.UserToken = message;
                saea.Completed += SendMessageToKnownServerCompletedCallback;
                mSockBroadcastSender.SendToAsync(saea);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SendMessageToKnownServerCompletedCallback(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine($"Sent: {e.UserToken}{Environment.NewLine}Server:{e.RemoteEndPoint}");
            OnRaisePrintStringEvent(new PrintStringEventArgs($"Sent: {e.UserToken}{Environment.NewLine}Server:{e.RemoteEndPoint}"));
        }
    }
}
