using System;

namespace UDP_Asynchronous_Chat
{
    public class ImageReceived : EventArgs
    {
        public readonly string fileName;
        public readonly byte[] fileData;
        public readonly string message;

        public ImageReceived(string fileName, byte[] fileData, string message)
        {
            this.fileName = fileName;
            this.fileData = fileData;
            this.message = message;
        }
    }
}