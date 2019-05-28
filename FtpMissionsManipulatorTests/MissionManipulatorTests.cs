﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using FtpMissionsManipulator;
using FtpMissionsManipulator.MissionSource;
using Moq;
using NUnit.Framework;

namespace FtpMissionsManipulatorTests
{
    [TestFixture]
    public class MissionManipulatorTests
    {
        private MissionManipulator _sut;
        private Mock<IMissionsSource> _missionSourceMock;
        private string _liveDirectory;
        private IEnumerable<Mission> _liveMissions;
        private string _pendingDirectory;
        private IEnumerable<Mission> _pendingMissions;
        private Mission _updatedMission;
        private Mission _oldMission;
        private Mission _anotherUpdatedMission;

        [SetUp]
        public void Setup()
        {
            _liveDirectory = MissionManipulator.LiveDirectory;
            _pendingDirectory = MissionManipulator.FinalDirectory;
            _oldMission = new Mission("CO42_Test_Mission_Name_v1.2.Chernarus.pbo",
                MissionType.Coop,
                42,
                "Test_Mission_Name",
                new MissionVersion("v1.2",
                    new MissionVersionComparer()),
                "Chernarus");

            _updatedMission = new Mission("CO46_Test_Mission_Name_v2.1.Chernarus.pbo",
                MissionType.Coop,
                46,
                "Test_Mission_Name",
                new MissionVersion("v2.1",
                    new MissionVersionComparer()),
                "Chernarus");
            _anotherUpdatedMission = new Mission("CO46_Test_Mission_Name_v2.2.Chernarus.pbo",
                MissionType.Coop,
                46,
                "Test_Mission_Name",
                new MissionVersion("v2.2",
                    new MissionVersionComparer()),
                "Chernarus");

            _liveMissions = new[]
            {
                _oldMission,
                new Mission(),
            };
            _pendingMissions = new[]
            {
                new Mission(),
                _updatedMission,
            };

            _missionSourceMock = new Mock<IMissionsSource>();
            _missionSourceMock
                .Setup(m => m.GetMissionsFromDirectory(_liveDirectory))
                .Returns(_liveMissions);
            _missionSourceMock
                .Setup(m => m.GetMissionsFromDirectory(_pendingDirectory))
                .Returns(_pendingMissions);

            _sut = new MissionManipulator(_missionSourceMock.Object);
        }

        [Test]
        public void GetLiveMissions_MissionSourceProvidesMissions_CorrectDirectoryUsed()
        {
            var result = _sut.LiveMissions;

            _missionSourceMock.Verify(m => m.GetMissionsFromDirectory(_liveDirectory), Times.Once);
        }

        [Test]
        public void GetLiveMissions_MissionSourceProvidesMissions_ReturnedMissionsRetrievedFromSource()
        {
            var result = _sut.LiveMissions;

            CollectionAssert.AreEquivalent(_liveMissions, result);
        }

        [Test]
        public void GetLiveMissions_MissionSourceProvidesMissions_SourceNotCalledOnSubsequentCalls()
        {
            var unused = _sut.LiveMissions;
            unused = _sut.LiveMissions;

            _missionSourceMock.Verify(m => m.GetMissionsFromDirectory(_liveDirectory), Times.Once);
        }

        [Test]
        public void GetPendingMissions_MissionSourceProvidesMissions_CorrectDirectoryUsed()
        {
            var unused = _sut.PendingMissions;

            _missionSourceMock.Verify(m => m.GetMissionsFromDirectory(_pendingDirectory), Times.Once);
        }

        [Test]
        public void GetPendingMissions_MissionSourceProvidesMissions_ReturnedMissionsRetrievedFromSource()
        {
            var result = _sut.PendingMissions;

            CollectionAssert.AreEquivalent(_pendingMissions, result);
        }

        [Test]
        public void GetPendingMissions_MissionSourceProvidesMissions_SourceNotCalledOnSubsequentCalls()
        {
            var unused = _sut.PendingMissions;
            unused = _sut.PendingMissions;

            _missionSourceMock.Verify(m => m.GetMissionsFromDirectory(_pendingDirectory), Times.Once);
        }

        [Test]
        public void GetUpdatedMissions_OneMissionHasUpdatePending_OneMissionUpdateCorrectlyCreated()
        {
            var result = _sut.GetUpdatedMissions().ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(_updatedMission, result.First().NewMission);
            Assert.AreEqual(_oldMission, result.First().OldMission);
        }

        [Test]
        public void GetUpdatedMissions_NoUpdatesPending_NoMissionUpdatesCreated()
        {
            _missionSourceMock
                .Setup(m => m.GetMissionsFromDirectory(_pendingDirectory))
                .Returns(Enumerable.Empty<Mission>());

            var result = _sut.GetUpdatedMissions().ToArray();

            Assert.That(result.IsNullOrEmpty());
        }

        [Test]
        public void GetUpdatedMissions_NewMissionPending_NoMissionUpdatesCreated()
        {
            _missionSourceMock
                .Setup(m => m.GetMissionsFromDirectory(_pendingDirectory))
                .Returns(new[]
                {
                    new Mission(),
                });

            var result = _sut.GetUpdatedMissions().ToArray();

            Assert.That(result.IsNullOrEmpty());
        }

        [Test]
        public void GetUpdatedMissions_MultipleVersionsOfSameMissionPending_UpdateForEachCreated()
        {
            var expected = new[]
            {
                new MissionUpdate(_oldMission, _updatedMission),
                new MissionUpdate(_oldMission, _anotherUpdatedMission),
            };

            _missionSourceMock
                .Setup(m => m.GetMissionsFromDirectory(_pendingDirectory))
                .Returns(new[]
                {
                    _updatedMission,
                    _anotherUpdatedMission,
                    new Mission(), 
                });

            var result = _sut.GetUpdatedMissions().ToArray();

            CollectionAssert.AreEquivalent(expected, result);
        }
    }
}