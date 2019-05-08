using System;
using System.Linq;

namespace FtpMissionsManipulator
{
    public class MissionVersionComparer : IMissionVersionComparer
    {
        private int[] GetNumberRepresentation(string originalRepresentation)
        {
            var versionText = originalRepresentation.TrimStart('v', 'V');
            var numbers = versionText.Split('.');
            return numbers.Select(n =>
            {
                var hasParsed = int.TryParse(n, out var parsed);
                if (!hasParsed)
                    throw new ArgumentException(
                        "Provided version string is invalid (isn't exclusively numbers separated by periods)");
                return parsed;

            }).ToArray();
        }

        public int Compare(MissionVersion x, MissionVersion y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            var xN = GetNumberRepresentation(x.TextRepresentation);
            var yN = GetNumberRepresentation(y.TextRepresentation);
            for (var i = 0; i < Math.Max(xN.Length, yN.Length); i++)
            {
                var xCurrent = i < xN.Length ? xN[i] : 0;
                var yCurrent = i < yN.Length ? yN[i] : 0;

                if (xCurrent != yCurrent)
                    return xCurrent - yCurrent;
            }

            return 0;
        }

        public bool IsFormatCorrect(MissionVersion version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            try
            {
                GetNumberRepresentation(version.TextRepresentation);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}