using System;
using System.Collections.Generic;
using FtpMissionsManipulator;
using FtpMissionsManipulator.MissionSource;
using Moq;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests.MissionSource
{
    [TestFixture]
    public class CachedFtpConnectionTests
    {
        private CachedFtpConnection _sut;
        private Mock<IFtpConnection> _connectionMock;
        private Mock<ITimeProvider> _timeMock;
        private DateTime _time;

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

        [Test]
        public void GetStringResponse_InnerObjectCorrectlySet_InnerConnectionInvoked()
        {
            _sut.GetStringResponse("test");

            _connectionMock
                .Verify(m => m.GetStringResponseAsync("test"), Times.Once);
        }

        [Test]
        public void GetStringResponse_SecondRequestWithinCacheTime_InnerNotInvoked()
        {
            _sut.GetStringResponse("test");
            _time += TimeSpan.FromSeconds(1);

            _sut.GetStringResponse("test");

            _connectionMock
                .Verify(m => m.GetStringResponseAsync("test"), Times.Once);
        }

        [Test]
        public void GetStringResponse_SecondRequestAfterCacheTime_InnerInvokedAgain()
        {
            _sut.GetStringResponse("test");
            _time += TimeSpan.FromSeconds(5);

            _sut.GetStringResponse("test");

            _connectionMock
                .Verify(m => m.GetStringResponseAsync("test"), Times.Exactly(2));
        }
    }
}