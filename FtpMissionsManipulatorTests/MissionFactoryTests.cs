using System;
using FtpMissionsManipulator;
using Moq;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class MissionFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            _parserMock = new Mock<IMissionFilenameParser>();
            _parserMock
                .Setup(m => m.IsMissionNameFormatValid(_validMissionName))
                .Returns(true);
            _parserMock
                .Setup(m => m.IsMissionNameFormatValid(_invalidMissionName))
                .Returns(false);
            _parserMock
                .Setup(m => m.GetSize(It.IsAny<string>()))
                .Returns(_mockSize);
            _parserMock
                .Setup(m => m.GetMissionType(It.IsAny<string>()))
                .Returns(_mockType);
            _parserMock
                .Setup(m => m.GetName(It.IsAny<string>()))
                .Returns(_mockName);
            _parserMock
                .Setup(m => m.GetTerrain(It.IsAny<string>()))
                .Returns(_mockTerrain);
            _parserMock
                .Setup(m => m.GetVersion(It.IsAny<string>()))
                .Returns(_mockVersion);
            _sut = new MissionFactory(_parserMock.Object);
            _mockVersion = new MissionVersion("TestVersion", new Mock<IMissionVersionComparer>().Object);
        }

        private Mock<IMissionFilenameParser> _parserMock;
        private MissionFactory _sut;
        private readonly string _validMissionName = "CO10_Test_v1.terrain.pbo";
        private readonly string _invalidMissionName = "CO_Test.terrain.pbo";
        private readonly int _mockSize = 42;
        private readonly MissionType _mockType = MissionType.Coop;
        private readonly string _mockName = "TestName";
        private readonly string _mockTerrain = "TestTerrain";
        private MissionVersion _mockVersion;

        [Test]
        public void GetMission_InvalidInput_ArgumentExceptionThrown()
        {
            Assert.Throws<ArgumentException>(() => _sut.GetMission(_invalidMissionName));
        }

        [Test]
        public void GetMission_ValidInput_FullMissionNameSet()
        {
            var result = _sut.GetMission(_validMissionName);

            Assert.AreEqual(_validMissionName, result.FullName);
        }

        [Test]
        public void GetMission_ValidInput_NameFromParserSet()
        {
            var result = _sut.GetMission(_validMissionName);

            Assert.AreEqual(_mockName, result.Name);
        }

        [Test]
        public void GetMission_ValidInput_ParserCalled()
        {
            _sut.GetMission(_validMissionName);

            _parserMock.Verify(m => m.IsMissionNameFormatValid(_validMissionName));
        }

        [Test]
        public void GetMission_ValidInput_SizeFromParserSet()
        {
            var result = _sut.GetMission(_validMissionName);

            Assert.AreEqual(_mockSize, result.Size);
        }

        [Test]
        public void GetMission_ValidInput_TerrainFromParserSet()
        {
            var result = _sut.GetMission(_validMissionName);

            Assert.AreEqual(_mockTerrain, result.Terrain);
        }

        [Test]
        public void GetMission_ValidInput_TypeFromParserSet()
        {
            var result = _sut.GetMission(_validMissionName);

            Assert.AreEqual(_mockType, result.Type);
        }

        [Test]
        public void GetMission_ValidInput_VersionFromParserSet()
        {
            var result = _sut.GetMission(_validMissionName);

            Assert.AreEqual(_mockVersion, result.Version);
        }
    }
}