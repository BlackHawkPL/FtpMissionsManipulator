using System;
using FtpMissionsManipulator;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class MissionVersionComparerTests
    {
        private MissionVersionComparer _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new MissionVersionComparer();
        }

        private MissionVersion GetMissionVersion(string version)
        {
            return new MissionVersion(version, _sut);
        }

        [TestCase("v1", "v1.0", ExpectedResult = 0)]
        [TestCase("v1", "v2", ExpectedResult = -1)]
        [TestCase("v1", "v1.1", ExpectedResult = -1)]
        [TestCase("v1", "v0.9", ExpectedResult = 1)]
        [TestCase("v1.1", "v1.2", ExpectedResult = -1)]
        [TestCase("v123", "v1.123", ExpectedResult = 1)]
        [TestCase("v1.1.1", "v1.0", ExpectedResult = 1)]
        [TestCase("v1.5", "v2.0", ExpectedResult = -1)]
        public int Compare_Test(string firstVersion, string secondVersion)
        {
            var firstMissionVersion = new MissionVersion(firstVersion, _sut);
            var secondMissionVersion = new MissionVersion(secondVersion, _sut);
            var result = _sut.Compare(firstMissionVersion, secondMissionVersion);

            if (result == 0)
                return result;

            return result / Math.Abs(result);
        }

        [TestCase("v3", ExpectedResult = true)]
        [TestCase("V3", ExpectedResult = true)]
        [TestCase("v1.2.3.4", ExpectedResult = true)]
        [TestCase("v123", ExpectedResult = true)]
        [TestCase("v123.567.890", ExpectedResult = true)]
        [TestCase("v", ExpectedResult = false)]
        [TestCase("vx", ExpectedResult = false)]
        [TestCase("v1.2.3a", ExpectedResult = false)]
        [TestCase("v1,2,3", ExpectedResult = false)]
        public bool IsFormatCorrect_Test(string version)
        {
            var mv = new MissionVersion(version, _sut);

            var result = _sut.IsFormatCorrect(mv);

            return result;
        }

        [Test]
        public void Compare_CallingWithInvalidVersion_ArgumentExceptionThrown()
        {
            var a = GetMissionVersion("v1.2a");
            var b = GetMissionVersion("v1.2b");

            Assert.Throws<ArgumentException>(() =>
            {
                var unused = _sut.Compare(a, b);
            });
        }

        [Test]
        public void Compare_FirstArgumentIsNull_ArgumentNullExceptionThrown()
        {
            var a = GetMissionVersion("v1.2a");

            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = _sut.Compare(null, a);
            });
        }

        [Test]
        public void Compare_SecondArgumentIsNull_ArgumentNullExceptionThrown()
        {
            var a = GetMissionVersion("v1.2a");

            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = _sut.Compare(a, null);
            });
        }

        [Test]
        public void IsFormatCorrect_ArgumentIsNull_ArgumentNullExceptionThrown()
        {
            var mv = GetMissionVersion("v1.2");

            Assert.Throws<ArgumentNullException>(() => _sut.IsFormatCorrect(null));
        }
    }
}