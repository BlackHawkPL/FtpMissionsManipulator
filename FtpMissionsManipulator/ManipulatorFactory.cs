using System.Threading.Tasks;
using Autofac;
using FtpMissionsManipulator.MissionSource;

namespace FtpMissionsManipulator
{
    public class ManipulatorFactory : IManipulatorFactory
    {
        public async Task<IMissionsManipulator> SetupAsync(string host, int port, string username, string password)
        {
            var connection = new FtpConnection();

            var couldConnect = await connection
                .TryConnectAsync(host, port, username, password)
                .ConfigureAwait(false);

            if (!couldConnect)
                return null;

            var builder = new ContainerBuilder();

            builder.RegisterType<MissionsManipulator>();
            builder.RegisterType<FtpMissionsSource>()
                .As<IMissionsSource>();
            builder.RegisterType<MissionFactory>()
                .As<IMissionFactory>();
            builder.RegisterType<MissionFilenameParser>()
                .As<IMissionFilenameParser>();
            builder.RegisterType<MissionVersionComparer>()
                .As<IMissionVersionComparer>();
            builder.RegisterType<MissionVersionFactory>()
                .As<IMissionVersionFactory>();
            builder.RegisterType<TimeProvider>()
                .As<ITimeProvider>();
            builder.RegisterInstance(connection)
                .As<IFtpConnection>();
            builder.RegisterDecorator<CachedFtpConnection, IFtpConnection>();
            builder.RegisterDecorator<ConcurrentFtpConnection, IFtpConnection>();

            return builder.Build().Resolve<MissionsManipulator>();
        }
    }
}