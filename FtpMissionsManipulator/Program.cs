using System;
using System.IO;
using System.Net;

namespace FtpMissionsManipulator
{
    class Program
    {
        static void Main(string[] args)
        {
            var request = WebRequest.Create("ftp://srv1.unitedoperations.net/_FINAL") as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential("uomissions", "grazebox");

            var response = request.GetResponse() as FtpWebResponse;

            var responseStream = response.GetResponseStream();
            var reader = new StreamReader(responseStream);
            var result = reader.ReadToEnd();
            Console.WriteLine(result);
            response.Close();
            reader.Close();

            Console.WriteLine("Hello World!");
        }
    }
}
