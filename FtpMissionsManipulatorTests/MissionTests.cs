using FtpMissionsManipulator;
using Moq;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class MissionTests
    {
        [Test]
        public void Equals_DifferentMissions_ReturnsFalse()
        {
            var first = new Mission("CO42_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");
            var second = new Mission("CO42_Different_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");

            var result = first.Equals(second);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SameMission_ReturnsTrue()
        {
            var mission = new Mission("CO42_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");

            var result = mission.Equals(mission);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_MissionsWithSameName_ReturnsTrue()
        {
            var first = new Mission("CO42_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");
            var second = new Mission("CO42_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");

            var result = first.Equals(second);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OtherMissionIsNull_ReturnsFalse()
        {
            var first = new Mission("CO42_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");

            var result = first.Equals(null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_TwoEmptyMissions_ReturnsFalse()
        {
            var first = new Mission();
            var second = new Mission();

            var result = first.Equals(second);

            Assert.IsFalse(result);
        }
    }
}