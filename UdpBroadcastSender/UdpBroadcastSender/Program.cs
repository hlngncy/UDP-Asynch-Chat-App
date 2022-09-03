using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace UdpBroadcastSender
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket sockBroadcaster = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sockBroadcaster.EnableBroadcast = true;
            IPEndPoint broadcastEP = new IPEndPoint(IPAddress.Parse("255.255.255.255"),23000);
            byte[] buffer = new byte[] {0x0D,0X0A};
            string strUserInput = string.Empty;
            IPEndPoint ipepSender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epSender = (EndPoint)ipepSender;
            string textReceived = string.Empty;
            int nCountReceived = 0;
            try
            {
                sockBroadcaster.Bind(new IPEndPoint(IPAddress.Any,0));
                while (true)
                {
                    Console.Write("Please input a string to send broadcast type <EXIT> to close");
                    strUserInput = Console.ReadLine();
                    if (strUserInput.Equals("<EXIT>"))
                    {
                        break;
                    }

                    buffer = Encoding.ASCII.GetBytes(strUserInput);
                    sockBroadcaster.SendTo(buffer, broadcastEP);
                    if (strUserInput.Equals("<ECHO>"))
                    {
                        nCountReceived = sockBroadcaster.ReceiveFrom(buffer, ref epSender);
                        textReceived = Encoding.ASCII.GetString(buffer, 0, nCountReceived);
                        Console.WriteLine("Number of Bytes Received: " + nCountReceived);
                        Console.WriteLine("Text Received: " + textReceived);
                        Console.WriteLine("Received From: " + epSender.ToString());
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                }
                sockBroadcaster.Shutdown(SocketShutdown.Both);
                sockBroadcaster.Close();

            }
            catch (Exception excp)
            {

                Console.WriteLine(excp.ToString());
            }
        }
    }
}