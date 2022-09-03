using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace UdpBroadcastReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket sockBroadcastReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); 
            IPEndPoint ipepLocal = new IPEndPoint(IPAddress.Any, 23000);
            byte[] buffer = new byte[512];
            int nCountReceived= 0;
            string textReceived = string.Empty;
            IPEndPoint ipepSender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epSender = (EndPoint)ipepSender;
            try
            {
                sockBroadcastReceiver.Bind(ipepLocal);
                while (true)
                {
                    nCountReceived = sockBroadcastReceiver.ReceiveFrom(buffer, ref epSender);
                    textReceived = Encoding.ASCII.GetString(buffer, 0, nCountReceived);
                    Console.WriteLine("Number of Bytes Received: " + nCountReceived);
                    Console.WriteLine("Text Received: " + textReceived);
                    Console.WriteLine("Received From: " + epSender.ToString());
                    if (textReceived.Equals("<ECHO>"))
                    {
                        sockBroadcastReceiver.SendTo(buffer, 0, nCountReceived, SocketFlags.None, epSender);
                        Console.WriteLine("Text Echoed back...");
                    }
                    Array.Clear(buffer, 0, buffer.Length);

                }

            }
            catch (Exception excp)
            {

                Console.WriteLine(excp.ToString());
            }
        }
    }
}