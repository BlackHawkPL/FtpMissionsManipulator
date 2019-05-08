using System;
using FtpMissionsManipulator;
using Moq;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class MissionFilenameParserTests
    {
        private IMissionVersionFactory _versionFactory;
        private IMissionVersionComparer _missionVersionComparer;
        private MissionFilenameParser _sut;
        private Mock<IMissionVersionFactory> _versionFactoryMock;
        private Mock<IMissionVersionComparer> _versionComparerMock;

        [SetUp]
        public void Setup()
        {
            _versionFactoryMock = new Mock<IMissionVersionFactory>();
            _versionFactory = _versionFactoryMock.Object;
            _versionComparerMock = new Mock<IMissionVersionComparer>();
            _missionVersionComparer = _versionComparerMock.Object;
            _sut = new MissionFilenameParser(_missionVersionComparer, _versionFactory);
        }


        [TestCase("CO10_Test_v3.WL_Rosche.pbo", "v3")]
        [TestCase("CO10_Test_V3.Chernarus.pbo", "V3")]
        [TestCase("CO10_Test_v1.2.3.4.Altis.pbo", "v1.2.3.4")]
        [TestCase("CO10_Test_v123.Tanoa.pbo", "v123")]
        [TestCase("CO10_Test_V1.2.3.Terrain.pbo", "V1.2.3")]
        [TestCase("CO10_Test_v123.567.890.Sample_Long_Terrain_Name.pbo", "v123.567.890")]
        [TestCase("CO10_TEST_V3.WL_ROSCHE.PBO", "V3")]
        [TestCase("CO10_TEST_V3.WL_ROSCHE.Pbo", "V3")]
        public void GetVersion_ValidMissionNamesAndVersion_CorrectVersionReturned(string mission, string expected)
        {
            _sut.GetVersion(mission);

            _versionFactoryMock.Verify(m => m.GetMissionVersion(expected, _missionVersionComparer));
        }

        [TestCase("C10_Test_v1,2.Terrain.pbo")]
        [TestCase("CO10_Test_v1a.Terrain.pbo")]
        [TestCase("CO10_Test.Terrain.pbo")]
        [TestCase("Test.Terrain.pbo")]
        [TestCase("CO10_Test_v1.Terrain")]
        [TestCase("CO10_Test_v1.pbo")]
        [TestCase("Test")]
        [TestCase("_")]
        [TestCase(".")]
        [TestCase("_.")]
        [TestCase(".pbo")]
        [TestCase("_.pbo")]
        [TestCase(".pbo_")]
        public void GetVersion_MalformedMissionName_ArgumentExceptionThrown(string mission)
        {
            Assert.Throws<ArgumentException>(() => _sut.GetVersion(mission));
        }

        [TestCase("CO10_Test_v1.Terrain.pbo", ExpectedResult = MissionType.Coop)]
        [TestCase("TVT10_Test_v1.terrain.pbo", ExpectedResult = MissionType.TVT)]
        [TestCase("COTVT20_Test_v1.terrain.pbo", ExpectedResult = MissionType.COTVT)]
        [TestCase("LOL10_Test_v1.terrain.pbo", ExpectedResult = MissionType.LOL)]
        [TestCase("tvt10_test_v1.terrain.pbo", ExpectedResult = MissionType.TVT)]
        [TestCase("Lol20_test_v1.terrain.pbo", ExpectedResult = MissionType.LOL)]
        [TestCase("TvT10_Test_v1.terrain.pbo", ExpectedResult = MissionType.TVT)]
        public MissionType GetMissionType_ValidMissionName_CorrectTypeReturned(string mission)
        {
            return _sut.GetMissionType(mission);
        }

        [TestCase("XD10_Test.v1.Terrain.pbo")]
        [TestCase("10_Test.v1.Terrain.pbo")]
        [TestCase("Test_v1.terrain.pbo")]
        [TestCase("coop10_test_v1.terrain.pbo")]
        [TestCase("co_mission_v1.terrain.pbo")]
        [TestCase("tvtMissionName_v1.terrain.pbo")]
        [TestCase("Tv10_Test")]
        public void GetMissionType_MalformedMissionName_ArgumentExceptionThrown(string mission)
        {
            Assert.Throws<ArgumentException>(() => _sut.GetMissionType(mission));
        }

        [TestCase("TVT10_Test_v1.Terrain.pbo", ExpectedResult = "Terrain")]
        [TestCase("TVT10_Test_v1.Terrain_Long_Name.pbo", ExpectedResult = "Terrain_Long_Name")]
        [TestCase("TVT10_Test_v1.2.3.Terrain.pbo", ExpectedResult = "Terrain")]
        [TestCase("TVT10_Test_v1.2.3.Terrain.PBO", ExpectedResult = "Terrain")]
        [TestCase("TVT10_Test_v1.2.3.Terrain.Pbo", ExpectedResult = "Terrain")]
        public string GetTerrain_ValidMissionName_CorrectTerrainReturned(string mission)
        {
            return _sut.GetTerrain(mission);
        }

        [TestCase("CO10_Name_v1.pbo")]
        [TestCase("CO10_mission.pbo")]
        [TestCase("CO10_name_v1.terrain")]
        public void GetTerrain_MalformedMissionName_ArgumentExceptionThrown(string mission)
        {
            Assert.Throws<ArgumentException>(() => _sut.GetTerrain(mission));
        }

        [TestCase("CO10_Name_v1.terrain.pbo", ExpectedResult = 10)]
        [TestCase("TVT09_Test_v1.terrain.pbo", ExpectedResult = 9)]
        [TestCase("COTVT9_Test_v1.terrain.pbo", ExpectedResult = 9)]
        [TestCase("TVT10_10_Test_v1.terrain.pbo", ExpectedResult = 10)]
        public int GetSize_ValidMissionName_CorrectSizeReturned(string mission)
        {
            return _sut.GetSize(mission);
        }

        [TestCase("CO0_Name_v1.terrain.pbo")]
        [TestCase("TVT-9_Test_v1.terrain.pbo")]
        [TestCase("9_Test_v1.terrain.pbo")]
        public void GetSize_MalformedMissionName_ArgumentExceptionThrown(string mission)
        {
            Assert.Throws<ArgumentException>(() => _sut.GetSize(mission));
        }

        [TestCase("CO10_TestName_v1.terrain.pbo", ExpectedResult = "TestName")]
        [TestCase("TVT10_Name_v1.terrain.pbo", ExpectedResult = "Name")]
        [TestCase("LOL10_Multi_Word_Name_v1.terrain.pbo", ExpectedResult = "Multi_Word_Name")]
        [TestCase("COTVT20_Żółć_v1.terrain.pbo", ExpectedResult = "Żółć")]
        public string GetName_ValidMissionName_CorrectNameReturned(string mission)
        {
            return _sut.GetName(mission);
        }

        [TestCase("CO10_v1.terrain.pbo")]
        [TestCase("Name.pbo")]
        [TestCase("name.island.pbo")]
        [TestCase("TVT10test_v1.terrain.pbo")]
        public void GetName_MalformedMissionName_ArgumentExceptionThrown(string mission)
        {
            Assert.Throws<ArgumentException>(() => _sut.GetName(mission));
        }

        [TestCase("CO10_Test_v1.terrain.pbo")]
        [TestCase("TVT10_A_Very_Long_Mission_Name_v1.2.3.50.terrain_name.pbo")]
        public void IsMissionNameFormatValid_ValidMissionNames_ReturnsTrue(string mission)
        {
            Assert.IsTrue(_sut.IsMissionNameFormatValid(mission));
        }

        [TestCase("10_Test_v1.terrain.pbo")]
        [TestCase("co_test_v1.terrain.pbo")]
        [TestCase("tvt10_v1.terrain.pbo")]
        [TestCase("cotvt20_mission_name.terrain.pbo")]
        [TestCase("lol50_name_v1.2.3.pbo")]
        [TestCase("co69_test_v1.2.terrain")]
        public void IsMissionNameFormatValid_MalformedMissionNames_ReturnsFalse(string mission)
        {
            Assert.IsFalse(_sut.IsMissionNameFormatValid(mission));
        }
    }
}