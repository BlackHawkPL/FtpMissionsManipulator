using System;

namespace FtpMissionsManipulator
{
    public class FtpException : Exception
    {
        public FtpException(string msg) : base(msg)
        {
        }
    }
}