using System.Threading.Tasks;

namespace FtpMissionsManipulator
{
    public interface IMissionSourceFactory
    {
        Task<IMissionsSource> SetupAsync(string host, int port, string username, string password);
    }
}