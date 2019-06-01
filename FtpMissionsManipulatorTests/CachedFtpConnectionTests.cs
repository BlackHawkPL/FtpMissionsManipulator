using System;
using FtpMissionsManipulator.MissionSource;
using Moq;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class CachedFtpConnectionTests
    {
        [SetUp]
        public void Setup()
        {
            _timeMock = new Mock<ITimeProvider>();
            _time = DateTime.Parse("2019-05-31T12:00");
            _timeMock
                .Setup(m => m.GetCurrentTime())
                .Returns(() => _time);

            _connectionMock = new Mock<IFtpConnection>();
            _sut = new CachedFtpConnection(_connectionMock.Object, _timeMock.Object, 2);
        }

        private CachedFtpConnection _sut;
        private Mock<IFtpConnection> _connectionMock;
        private Mock<ITimeProvider> _timeMock;
        private DateTime _time;

        [Test]
        public void GetStringResponse_InnerObjectCorrectlySet_InnerConnectionInvoked()
        {
            var unused = _sut.GetDirectoryListingAsync("test").Result;

            _connectionMock
                .Verify(m => m.GetDirectoryListingAsync("test"), Times.Once);
        }

        [Test]
        public void GetStringResponse_SecondRequestAfterCacheTime_InnerInvokedAgain()
        {
            var unused = _sut.GetDirectoryListingAsync("test").Result;
            _time += TimeSpan.FromSeconds(5);

            var unused1 = _sut.GetDirectoryListingAsync("test").Result;

            _connectionMock
                .Verify(m => m.GetDirectoryListingAsync("test"), Times.Exactly(2));
        }

        [Test]
        public void GetStringResponse_SecondRequestWithinCacheTime_InnerNotInvoked()
        {
            var unused = _sut.GetDirectoryListingAsync("test").Result;
            _time += TimeSpan.FromSeconds(1);

            var unused1 = _sut.GetDirectoryListingAsync("test").Result;

            _connectionMock
                .Verify(m => m.GetDirectoryListingAsync("test"), Times.Once);
        }
    }
}