using System;

namespace FtpMissionsManipulator.MissionSource
{
    public interface ITimeProvider
    {
        DateTime GetCurrentTime();
    }
}