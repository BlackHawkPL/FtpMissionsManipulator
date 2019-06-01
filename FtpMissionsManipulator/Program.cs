using System;
using System.Configuration;
using System.Threading.Tasks;
using Autofac;
using FtpMissionsManipulator.MissionSource;

namespace FtpMissionsManipulator
{
    internal class Program
    {
        private static string _readLine;

        private static void Main()
        {
            var address = ConfigurationManager.AppSettings["address"];
            var username = ConfigurationManager.AppSettings["username"];
            var password = ConfigurationManager.AppSettings["password"];

            Console.WriteLine("Refresh settings?");
            var answer = Console.ReadKey();
            Console.WriteLine();

            if (char.ToLower(answer.KeyChar) == 'y' || address == null || username == null || password == null)
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Clear();
                address = SetSetting("address", config);
                username = SetSetting("username", config);
                password = SetSetting("password", config);
            }


            var builder = new ContainerBuilder();

            builder.RegisterType<MissionManipulator>();
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
            builder.RegisterInstance(new FtpConnection(address, username, password))
                .As<IFtpConnection>();
            builder.RegisterDecorator<CachedFtpConnection, IFtpConnection>();
            builder.RegisterDecorator<ConcurrentFtpConnection, IFtpConnection>();

            var manipulator = builder.Build().Resolve<MissionManipulator>();

            var tasks = new[]
            {
                PrintLiveAsync(manipulator),
                PrintFaultyAsync(manipulator),
                PrintUpdatedAsync(manipulator),
                PrintDuplicatesAsync(manipulator),
                manipulator.TestAsync()
            };
            Task.WhenAll(tasks).ConfigureAwait(false);

            Console.ReadKey();
        }

        private static async Task PrintDuplicatesAsync(MissionManipulator manipulator)
        {
            var missions = await manipulator.GetDuplicateMissionsFromLiveAsync().ConfigureAwait(false);
            Console.WriteLine("\nDuplicates are:");
            foreach (var duplicate in missions)
                Console.WriteLine($"D {duplicate.FullName}");
        }

        private static async Task PrintUpdatedAsync(MissionManipulator manipulator)
        {
            var missionUpdates = await manipulator.GetUpdatedMissionsAsync().ConfigureAwait(false);
            Console.WriteLine("\nMissions to update are:");
            foreach (var update in missionUpdates)
                Console.WriteLine(
                    $"U {update.NewMission.Name} from {update.OldMission.Version} to {update.NewMission.Version}");
        }

        private static async Task PrintFaultyAsync(MissionManipulator manipulator)
        {
            var objects = await manipulator.GetMissionsWithIncorrectNamesInLiveAsync().ConfigureAwait(false);
            Console.WriteLine("\nMissions with faulty names:");
            foreach (var mission in objects)
                Console.WriteLine("F " + mission);
        }

        private static async Task PrintLiveAsync(MissionManipulator manipulator)
        {
            var missions = await manipulator.GetLiveMissionsAsync().ConfigureAwait(false);
            Console.WriteLine("\nLive missions:");
            foreach (var mission in missions)
                Console.WriteLine("L " + mission.FullName);
        }

        private static string SetSetting(string name, Configuration configFile)
        {
            var settings = configFile.AppSettings.Settings;
            Console.WriteLine($"Enter {name}: ");
            _readLine = Console.ReadLine();
            settings.Add(name, _readLine);
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            return _readLine;
        }
    }
}