// Copyright (c) 2025 GooGuTeam
// Licensed under the AGPL-3.0 Licence. See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using static System.Int32;

namespace CustomRulesetGenerator
{
    public static class VersionHelper
    {
        public static int CompareVersionDates(string v1, string v2)
        {
            if (string.IsNullOrEmpty(v1))
            {
                return string.IsNullOrEmpty(v2) ? 0 : -1;
            }

            if (string.IsNullOrEmpty(v2))
            {
                return 1;
            }

            (DateTime date1, int patch1) = ParseVersion(v1);
            (DateTime date2, int patch2) = ParseVersion(v2);

            int dateCompare = DateTime.Compare(date1, date2);
            return dateCompare != 0 ? dateCompare : patch1.CompareTo(patch2);
        }

        private static (DateTime Date, int Patch) ParseVersion(string version)
        {
            // e.g. "2025.1019.2", "2025.1019"
            string[] parts = version.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 1)
                throw new FormatException($"Invalid version format: {version}");

            if (!DateTime.TryParseExact(parts[0] + parts[1], "yyyyMMdd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime date))
                throw new FormatException($"Failed to parse date: {version}");

            int patch = 0;
            if (parts.Length <= 2)
            {
                return (date, patch);
            }

            bool success = TryParse(parts[2], out patch);
            return !success ? throw new FormatException($"Failed to parse patch: {version}") : (date, patch);
        }
    }
}