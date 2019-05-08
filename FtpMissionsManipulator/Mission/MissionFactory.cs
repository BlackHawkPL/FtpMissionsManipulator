using System;

namespace FtpMissionsManipulator
{
    public class MissionFactory
    {
        private readonly IMissionFilenameParser _filenameParser;

        public MissionFactory(IMissionFilenameParser filenameParser)
        {
            _filenameParser = filenameParser;
        }

        public Mission GetMission(string missionName)
        {
            if (!_filenameParser.IsMissionNameFormatValid(missionName))
                throw new ArgumentException("Provided mission had incorrect format");

            return new Mission(missionName,
                _filenameParser.GetMissionType(missionName), _filenameParser.GetSize(missionName),
                _filenameParser.GetName(missionName), _filenameParser.GetVersion(missionName),
                _filenameParser.GetTerrain(missionName));
        }
    }
}