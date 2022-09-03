using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UDP_Asynchronous_Chat;

namespace UDP_Chat_Server_Form
{
    public partial class Form1 : Form
    {
        private UDP_Asynchronous_Chat.ChatServer mUdpChatServer;

        public Form1()
        {
            mUdpChatServer = new UDP_Asynchronous_Chat.ChatServer();
            mUdpChatServer.RaisePrintStringEvent += chatClient_PrintString;
            InitializeComponent();
        }

        private void chatClient_PrintString(object sender, PrintStringEventArgs e)
        {
            Action<string> print = PrintToTextBox;
            tbConsole.Invoke(print, new string[] { e.MessageToPrint });
        }

        private void PrintToTextBox(string obj)
        {
            tbConsole.Text += $"{Environment.NewLine}{DateTime.Now} - {obj} ";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mUdpChatServer.StartReceivingData();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}