using System.Threading.Tasks;
using Autofac;
using FtpMissionsManipulator.MissionSource;

namespace FtpMissionsManipulator
{
    public class MissionSourceFactory : IMissionSourceFactory
    {
        public async Task<IMissionsSource> SetupAsync(string host, int port, string username, string password)
        {
            var builder = new ContainerBuilder();

            var connection = new DelayedFtpConnection(new FtpConnection());
            var couldConnect = await connection
                .TryConnectAsync(host, port, username, password)
                .ConfigureAwait(false);

            if (!couldConnect)
                return null;

            builder.RegisterType<MissionsManipulator>().As<IMissionsManipulator>();
            builder.RegisterType<FtpMissionsSource>().As<IMissionsSource>();
            builder.RegisterType<MissionFactory>().As<IMissionFactory>();
            builder.RegisterType<MissionFilenameParser>().As<IMissionFilenameParser>();
            builder.RegisterType<MissionVersionComparer>().As<IMissionVersionComparer>();
            builder.RegisterType<MissionVersionFactory>().As<IMissionVersionFactory>();
            builder.RegisterType<TimeProvider>().As<ITimeProvider>();
            //builder.RegisterType<FtpConnection>().As<IFtpConnection>();
            //builder.RegisterDecorator<CachedFtpConnection, IFtpConnection>();
            //builder.RegisterDecorator<ConcurrentFtpConnection, IFtpConnection>();
                //builder.RegisterDecorator<DelayedFtpConnection, IFtpConnection>();

                builder.RegisterInstance(connection).As<IFtpConnection>();

            return builder.Build().Resolve<IMissionsSource>();
        }
    }
}