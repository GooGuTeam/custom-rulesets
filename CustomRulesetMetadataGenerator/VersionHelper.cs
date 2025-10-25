// Copyright (c) 2025 GooGuTeam
// Licensed under the AGPL-3.0 Licence. See the LICENCE file in the repository root for full licence text.

using System.Globalization;

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
            
            DateTime d1 = ParseVersionDate(v1);
            DateTime d2 = ParseVersionDate(v2);
            return DateTime.Compare(d1, d2);
        }

        private static DateTime ParseVersionDate(string version)
        {
            string[] formats = { "yyyy.M.d", "yyyy.MM.dd" };
            return DateTime.TryParseExact(version, formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out DateTime date)
                ? date
                : throw new FormatException($"Failed to parse version to date: {version}");
        }
    }
}