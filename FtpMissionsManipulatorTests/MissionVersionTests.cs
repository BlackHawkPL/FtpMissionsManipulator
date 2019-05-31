using FtpMissionsManipulator;
using NUnit.Framework;
using Moq;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class MissionVersionTests
    {
        private Mock<IMissionVersionComparer> _comparerMock;
        private MissionVersion _sut;
        private MissionVersionFactory _versionFactory;
        private const int ComparerResult = 0;
        private const string CorrectVersionString = "v1";
        private const string DifferentCorrectVersionString = "v2";

        public MissionVersion GetMissionVersion(string versionString)
        {
            return _versionFactory.GetMissionVersion(versionString, _comparerMock.Object);
        }

        [SetUp]
        public void Setup()
        {
            _versionFactory = new MissionVersionFactory();
            _comparerMock = new Mock<IMissionVersionComparer>();
            _comparerMock
                .Setup(c => c.Compare(It.IsAny<MissionVersion>(), It.IsAny<MissionVersion>()))
                .Returns(ComparerResult);

            _sut = GetMissionVersion(CorrectVersionString);
        }

        [Test]
        public void CompareTo_CorrectVersions_ReturnsResultFromComparer()
        {
            var b = GetMissionVersion(CorrectVersionString);

            var result = _sut.CompareTo(b);

            Assert.AreEqual(ComparerResult, result);
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

        [Test]
        public void Equals_OtherIsDifferentInstanceButSameState_ReturnsTrue()
        {
            var other = GetMissionVersion(CorrectVersionString);

            var result = _sut.Equals(other);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OtherIsSameObjectButCastDown_ReturnsTrue()
        {
            var other = GetMissionVersion(CorrectVersionString);

            var result = _sut.Equals((object) other);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OtherIsDifferentMissionVersionCastDownToObject_ReturnsTrue()
        {
            var other = GetMissionVersion(DifferentCorrectVersionString);

            var result = _sut.Equals((object) other);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OtherIsNullObject_ReturnsFalse()
        {
            object other = null;

            var result = _sut.Equals(other);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_OtherIsSameMissionVersionCastDownToObject_ReturnsTrue()
        {
            object other = _sut;

            var result = _sut.Equals(other);

            Assert.IsTrue(result);
        }

        [Test]
        public void GetHashCode_ObjectsHaveDifferentState_HashCodesAreDifferent()
        {
            var other = GetMissionVersion(DifferentCorrectVersionString);

            var result = _sut.GetHashCode();
            var otherResult = other.GetHashCode();

            Assert.AreNotEqual(result, otherResult);
        }

        [Test]
        public void GetHashCode_ObjectHaveSameState_HasCodesAreTheSame()
        {
            var other = GetMissionVersion(CorrectVersionString);

            var result = _sut.GetHashCode();
            var otherResult = other.GetHashCode();

            Assert.AreEqual(result, otherResult);
        }

        [Test]
        public void IsVersionCorrect_ValidState_UsesComparerImplementation()
        {
            _comparerMock
                .Setup(c => c.IsFormatCorrect(It.IsAny<MissionVersion>()))
                .Returns(true);

            var result = _sut.IsVersionCorrect();

            Assert.IsTrue(result);
        }

        [Test]
        public void IsVersionCorrect_ValidState_ComparerImplementationInvoked()
        {
            var result = _sut.IsVersionCorrect();

            _comparerMock.Verify(c => c.IsFormatCorrect(_sut), Times.Once);
        }

        [Test]
        public void ToString_VersionStringSet_VersionStringReturned()
        {
            var result = _sut.ToString();

            Assert.AreEqual(_sut.TextRepresentation, result);
        }
    }
}