using System;
using System.Threading.Tasks;
using FtpMissionsManipulator;
using FtpMissionsManipulator.MissionSource;
using Moq;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class FtpMissionsSourceTests
    {
        [SetUp]
        public void Setup()
        {
            _missionFactoryMock = new Mock<IMissionFactory>();
            _ftpConnectionMock = new Mock<IFtpConnection>();
            _directoryPrefix = "_Live/";
            _missionName1 = "mission1";
            _missionName2 = "mission2";
            _brokenMissionName1 = "brokenMissionName1";
            _brokenMissionName2 = "brokenMissionName2";
            _mission1 = new Mock<Mission>().Object;
            _mission2 = new Mock<Mission>().Object;
            SetupConnection();
            SetupFactory();
            _sut = new FtpMissionsSource(_missionFactoryMock.Object, _ftpConnectionMock.Object);
        }

        private Mock<IFtpConnection> _ftpConnectionMock;
        private Mock<IMissionFactory> _missionFactoryMock;
        private FtpMissionsSource _sut;
        private string _missionName1;
        private string _missionName2;
        private Mission _mission1;
        private Mission _mission2;
        private string _directoryPrefix;
        private string _brokenMissionName1;
        private string _brokenMissionName2;

        private void SetupConnection()
        {
            string GetFullLine(string missionName)
            {
                return _directoryPrefix + missionName + "\r\n";
            }

            _ftpConnectionMock
                .Setup(m => m.GetDirectoryListingAsync("test"))
                .Returns(Task.FromResult(GetFullLine(_missionName1) +
                                         GetFullLine(_missionName2) +
                                         GetFullLine(_brokenMissionName1) +
                                         GetFullLine(_brokenMissionName2)));
        }

        private void SetupFactory()
        {
            _missionFactoryMock
                .Setup(m => m.GetMission(_missionName1))
                .Returns(_mission1);
            _missionFactoryMock
                .Setup(m => m.GetMission(_missionName2))
                .Returns(_mission2);
            _missionFactoryMock
                .Setup(m => m.GetMission(It.IsIn(_brokenMissionName1, _brokenMissionName2)))
                .Throws<ArgumentException>();
        }

        [Test]
        public void GetFaultyFiles_FaultyFilesAvailable_CorrectlyReturned()
        {
            var result = _sut.GetFaultyFilesAsync("test").Result;

            CollectionAssert.AreEquivalent(new[] {_brokenMissionName1, _brokenMissionName2}, result);
        }

        [Test]
        public void GetMissionsFromDirectory_CorrectStringsSentToFactory_MissionsFromFactoryReturned()
        {
            var result = _sut.GetMissionsFromDirectoryAsync("test");

            CollectionAssert.AreEquivalent(new[] {_mission1, _mission2}, result.Result);
        }

        [Test]
        public void GetMissionsFromDirectory_FtpConnectionInjected_FtpConnectionCalledWithCorrectDirectory()
        {
            var directory = "test";
            var unused = _sut.GetMissionsFromDirectoryAsync(directory).Result;

            _ftpConnectionMock.Verify(m => m.GetDirectoryListingAsync(directory), Times.Once);
        }

        [Test]
        public void GetMissionsFromDirectory_MissionFactoryCantCreateMission_RemainingMissionsReturned()
        {
            _missionFactoryMock
                .Setup(m => m.GetMission(_missionName1))
                .Throws<ArgumentException>();

            var result = _sut.GetMissionsFromDirectoryAsync("test").Result;

            CollectionAssert.AreEquivalent(new[] {_mission2}, result);
        }

        [Test]
        public void GetMissionsFromDirectory_MissionFactoryInjected_MissionCreatedForEachEntry()
        {
            var unused = _sut.GetMissionsFromDirectoryAsync("test").Result;

            _missionFactoryMock
                .Verify(m => m.GetMission(_missionName1), Times.Once);
            _missionFactoryMock
                .Verify(m => m.GetMission(_missionName2), Times.Once);
        }
    }
}