using System.Threading.Tasks;

namespace FtpMissionsManipulator
{
    public interface IManipulatorFactory
    {
        Task<IMissionsManipulator> SetupAsync(string host, int port, string username, string password);
    }
}