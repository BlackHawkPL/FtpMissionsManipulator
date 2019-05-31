using System;
using System.Collections.Generic;
using System.Text;
using FtpMissionsManipulator;
using Moq;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class MissionUpdateTests
    {
        private Mission _differentMission;
        private Mission _mission;
        private Mission _sameMission;
        private MissionUpdate _update;
        private MissionUpdate _differentUpdate;
        private MissionUpdate _sameUpdate;

        [SetUp]
        public void Setup()
        {
            _mission = new Mission("CO42_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");
            _sameMission = new Mission("CO42_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");
            _differentMission = new Mission("CO42_Different_Test_v1.2.Chernarus.pbo", MissionType.Coop, 42, "Test", null, "Chernarus");

            _update = new MissionUpdate(_mission, _differentMission);
            _sameUpdate = new MissionUpdate(_mission, _differentMission);
            _differentUpdate = new MissionUpdate(_sameMission, _mission);
        }

        [Test]
        public void Equals_DifferentUpdates_ReturnsFalse()
        {
            var result = _update.Equals(_differentUpdate);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SameUpdate_ReturnsTrue()
        {
            var result = _update.Equals(_update);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_updatesWithSameName_ReturnsTrue()
        {

            var result = _update.Equals(_sameUpdate);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OtherUpdateIsNull_ReturnsFalse()
        {
            var result = _update.Equals(null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ComparingToEqualOtherUpdateCastAsObject_ReturnsTrue()
        {
            var result = _update.Equals((object)_sameUpdate);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_ComparingDifferentUpdateCastAsObject_ReturnsFalse()
        {
            var result = _update.Equals((object)_differentUpdate);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ComparingToNullCastAsObject_ReturnsFalse()
        {
            var result = _update.Equals((object)null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ComparingToItself_ReturnsTrue()
        {
            var result = _update.Equals((object)_update);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_ComparingToDifferentType_ReturnsFalse()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var result = _update.Equals(new StringBuilder());

            Assert.IsFalse(result);
        }

        [Test]
        public void GetHashCode_TwoDifferentUpdates_HashCodesDifferent()
        {
            var first = _update.GetHashCode();
            var second = _differentUpdate.GetHashCode();

            Assert.AreNotEqual(first, second);
        }
    }
}