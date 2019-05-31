using System;
using System.Text;
using FtpMissionsManipulator;
using Moq;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class MissionTests
    {
        private Mission _mission;
        private Mission _differentMission;
        private Mission _sameMission;

        [SetUp]
        public void Setup()
        {
            _mission = new Mission("CO42_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");
            _sameMission = new Mission("CO42_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");
            _differentMission = new Mission("CO42_Different_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");
        }

        [Test]
        public void Equals_DifferentMissions_ReturnsFalse()
        {
            var result = _mission.Equals(_differentMission);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SameMission_ReturnsTrue()
        {
            var result = _mission.Equals(_mission);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_MissionsWithSameName_ReturnsTrue()
        {

            var result = _mission.Equals(_sameMission);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OtherMissionIsNull_ReturnsFalse()
        {
            var result = _mission.Equals(null);

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

        [Test]
        public void Equals_ComparingToEqualOtherMissionCastAsObject_ReturnsTrue()
        {
            var result = _mission.Equals((object) _sameMission);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_ComparingDifferentMissionCastAsObject_ReturnsFalse()
        {
            var result = _mission.Equals((object) _differentMission);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ComparingToNullCastAsObject_ReturnsFalse()
        {
            var result = _mission.Equals((object) null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ComparingToItself_ReturnsTrue()
        {
            var result = _mission.Equals((object) _mission);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_ComparingToDifferentType_ReturnsFalse()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var result = _mission.Equals(new StringBuilder());

            Assert.IsFalse(result);
        }

        [Test]
        public void GetHashCode_TwoDifferentMissions_HashCodesDifferent()
        {
            var first = _mission.GetHashCode();
            var second = _differentMission.GetHashCode();

            Assert.AreNotEqual(first, second);
        }
    }
}