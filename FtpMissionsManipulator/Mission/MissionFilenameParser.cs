using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FtpMissionsManipulator
{
    public class MissionFilenameParser : IMissionFilenameParser
    {
        private readonly IMissionVersionComparer _missionMissionVersionComparer;
        private readonly IMissionVersionFactory _versionFactory;

        public MissionFilenameParser(IMissionVersionComparer missionVersionComparer, IMissionVersionFactory factory)
        {
            _versionFactory = factory;
            _missionMissionVersionComparer = missionVersionComparer ??
                                             throw new ArgumentNullException(nameof(missionVersionComparer));
        }

        public bool IsMissionNameFormatValid(string mission)
        {
            try
            {
                GetVersion(mission);
                GetName(mission);
                GetMissionType(mission);
                GetSize(mission);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public MissionVersion GetVersion(string mission)
        {
            try
            {
                var versionStart = GetVersionStart(mission);
                var versionEnd = Regex.Match(mission, @"(\.[^\.]*)\.pbo", RegexOptions.IgnoreCase).Index;
                var version = mission.Substring(versionStart, versionEnd - versionStart);
                if (!Regex.IsMatch(version, @"^[vV][\d\._]+$"))
                    throw new ArgumentException("Retrieved version didn't match regex");
                return _versionFactory.GetMissionVersion(version, _missionMissionVersionComparer);
            }
            catch (Exception e)
            {
                throw new ArgumentException(nameof(mission), e);
            }
        }

        private static int GetVersionStart(string mission)
        {
            var versionStart = mission
                                   .Split('.')
                                   .First()
                                   .ToLowerInvariant()
                                   .LastIndexOf("_v", StringComparison.Ordinal) + 1;
            return versionStart;
        }

        public string GetName(string mission)
        {
            try
            {
                var startIndex = mission.IndexOf('_') + 1;
                return mission.Substring(startIndex,
                    GetVersionStart(mission) - 1 - startIndex);
            }
            catch (Exception e)
            {
                throw new ArgumentException(nameof(mission), e);
            }
        }

        public MissionType GetMissionType(string mission)
        {
            mission = mission.ToUpper();

            if (!Regex.IsMatch(mission, @"^(TVT|CO|COTVT|LOL)\d+_"))
                throw new ArgumentException("Mission type didn't match regex");

            if (mission.StartsWith("COTVT"))
                return MissionType.COTVT;
            if (mission.StartsWith("TVT"))
                return MissionType.TVT;
            return mission.StartsWith("CO") ? MissionType.Coop : MissionType.LOL;
        }

        public int GetSize(string mission)
        {
            try
            {
                var match = Regex.Match(mission, @"(?<=TVT|CO|COTVT|LOL)+\d+(?=_)");
                if (!match.Success)
                    throw new ArgumentException("Mission size was not found");
                var result = int.Parse(match.Value);
                if (result <= 0)
                    throw new ArgumentException("Mission size was zero or negative");
                return result;
            }
            catch (Exception e)
            {
                throw new ArgumentException(nameof(mission), e);
            }
        }

        public string GetTerrain(string mission)
        {
            var match = Regex.Match(mission, @"(?<=\.)[^.]+(?=\.pbo)", RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new ArgumentException("Terrain was not found");

            return match.Value;
        }
    }
}