using FtpMissionsManipulator;
using NUnit.Framework;
using Moq;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class MissionVersionTests
    {
        private Mock<IVersionComparer> _comparerMock;
        private MissionVersion _sut;
        private const int ComparisonResult = 0;
        private const string CorrectVersionString = "v1";

        [SetUp]
        public void Setup()
        {
            _comparerMock = new Mock<IVersionComparer>();
            _comparerMock
                .Setup(c => c.Compare(It.IsAny<MissionVersion>(), It.IsAny<MissionVersion>()))
                .Returns(ComparisonResult);

            _sut = new MissionVersion(CorrectVersionString, _comparerMock.Object);

        }

        [Test]
        public void CompareTo_CorrectVersions_ReturnsResultFromComparer()
        {
            var b = new MissionVersion(CorrectVersionString, _comparerMock.Object);

            var result = _sut.CompareTo(b);

            Assert.AreEqual(ComparisonResult, result);
        }

        [Test]
        public void Equals_OtherIsNull_ReturnsFalse()
        {
            var result = _sut.Equals(null);

            Assert.AreEqual(false, result);
        }

        [Test]
        public void Equals_OtherIsNull_DoesNotInvokeComparer()
        {
            _sut.Equals(null);

            _comparerMock.Verify(c =>
                c.Compare(It.IsAny<MissionVersion>(), It.IsAny<MissionVersion>()),
                Times.Never());
        }

        [Test]
        public void Equals_OtherIsTheSameObject_ReturnsTrue()
        {
            var result = _sut.Equals(_sut);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void Equals_OtherIsDifferentType_ReturnsFalse()
        {
            var other = new object();

            var result = _sut.Equals(other);

            Assert.AreEqual(false, result);
        }
    }
}