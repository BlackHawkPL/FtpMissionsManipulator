using System;
using System.IO;
using System.Net;
using FtpMissionsManipulator.MissionSource;
using System.Configuration;
using Autofac;

namespace FtpMissionsManipulator
{
    class Program
    {
        private static string _readLine;

        static void Main(string[] args)
        {
            var address = ConfigurationManager.AppSettings["address"];
            var username = ConfigurationManager.AppSettings["username"];
            var password = ConfigurationManager.AppSettings["password"];

            Console.WriteLine("Refresh settings?");
            var answer = Console.ReadKey();
            Console.WriteLine();

            if (char.ToLower(answer.KeyChar) == 'y' || address == null || username == null || password == null)
            {
                address = SetSetting("address");
                username = SetSetting("username");
                password = SetSetting("password");
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
            builder.RegisterInstance(new FtpConnection(address, username, password))
                .As<IFtpConnection>();

            var manipulator = builder.Build().Resolve<MissionManipulator>();
            var liveMissions = manipulator.LiveMissions;

            foreach (var mission in liveMissions)
            {
                Console.WriteLine(mission.FullName);
            }

            Console.WriteLine("Missions to update are:");

            foreach (var update in manipulator.GetUpdatedMissions())
                Console.WriteLine($"Update {update.NewMission.Name} from {update.OldMission.Version} to {update.NewMission.Version}");

            Console.ReadKey();

            //var manipulator = new MissionManipulator(new FtpMissionsSource(
            //    new MissionFactory(new MissionFilenameParser(new MissionVersionComparer(),
            //        new MissionVersionFactory())), new FtpConnection()));
        }

        private static string SetSetting(string name)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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
