using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Autofac;
using FtpMissionsManipulator.MissionSource;

namespace FtpMissionsManipulator
{
    public class MissionSourceFactory : IMissionSourceFactory
    {
        private ILoggingProvider _loggingSource;

        public async Task<IMissionsSource> SetupAsync(string host, int port, string username, string password)
        {
            var builder = new ContainerBuilder();

            var connection = new DelayedFtpConnection(new FtpConnection());

            await connection
                .ConnectAsync(host, port, username, password);

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

            _loggingSource = new LoggingProvider();

            builder.RegisterInstance(_loggingSource).As<ILoggingProvider>();
            builder.RegisterDecorator<LoggingMissionSourceDecorator, IMissionsSource>();
            builder.RegisterInstance(connection).As<IFtpConnection>();

            var container = builder.Build();
            return container.Resolve<IMissionsSource>();
        }

        public ILoggingProvider GetLogsSource()
        {
            return _loggingSource;
        }
    }

    public class LoggingProvider : ILoggingProvider
    {
        readonly Subject<string> _subject = new Subject<string>();

        public IObservable<string> Logs => _subject.AsObservable();

        public void OnNext(string log)
        {
            _subject.OnNext(log);
        }
    }

    public interface ILoggingProvider
    {
        IObservable<string> Logs { get; }
        void OnNext(string log);
    }

    public class LoggingMissionSourceDecorator : IMissionsSource
    {
        private readonly ILoggingProvider _logger;
        private readonly IMissionsSource _inner;

        public LoggingMissionSourceDecorator(ILoggingProvider logger, IMissionsSource inner)
        {
            _logger = logger;
            _inner = inner;
        }

        public async Task<IEnumerable<Mission>> GetMissionsFromDirectoryAsync(string directory)
        {
            _logger.OnNext($"Getting missions from {directory}");
            return await _inner.GetMissionsFromDirectoryAsync(directory);
        }

        public async Task<IEnumerable<string>> GetFaultyFilesAsync(string directory)
        {
            _logger.OnNext($"Getting faulty files from {directory}");
            return await _inner.GetFaultyFilesAsync(directory);
        }

        public async Task<bool> MoveMissionToFolderAsync(Mission mission, string source, string destination)
        {
            _logger.OnNext($"Moving {mission} missions from {source} to {destination}");
            var success = await _inner.MoveMissionToFolderAsync(mission, source, destination);
            if (!success)
                _logger.OnNext($"Failed to move {mission}");
            return success;
        }

        public async Task DeleteMissionAsync(Mission mission, string directory)
        {
            _logger.OnNext($"Deleting {mission} from {directory}");
            await _inner.DeleteMissionAsync(mission, directory);
        }

        public async Task DeleteFileAsync(string fileName, string directory)
        {
            _logger.OnNext($"Deleting {fileName} from {directory}");
            await _inner.DeleteFileAsync(fileName, directory);
        }
    }
}