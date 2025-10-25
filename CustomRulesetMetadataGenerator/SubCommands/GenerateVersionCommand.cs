// Copyright (c) 2025 GooGuTeam
// Licensed under the AGPL-3.0 Licence. See the LICENCE file in the repository root for full licence text.

using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Game.Rulesets;
using System.Reflection;
using System.Security.Cryptography;

namespace CustomRulesetGenerator.SubCommands
{
    [Verb("ruleset-version",
        HelpText = "Generate rulesets version and MD5-hash for each ruleset found in the specified path.")]
    public class GenerateVersionOptions : BaseOptions
    {
        [Value(1, MetaName = "version", Required = true, HelpText = "The current version of the ruleset.")]
        public required string Version { get; set; }

        [Option("current", HelpText = "Existed JSON File path.", Required = false, Default = null)]
        public string? CurrentVersionFile { get; set; }
    }

    public class VersionEntry
    {
        [JsonProperty("latest-version")] public string LatestVersion { get; set; } = "";

        [JsonProperty("versions")] public Dictionary<string, string> Versions { get; set; } = new();
    }

    public class GenerateVersionCommand(GenerateVersionOptions options) : ISubCommand
    {
        public int Execute()
        {
            RulesetManager rulesetManager;
            try
            {
                rulesetManager = new RulesetManager(options.RulesetPath, false);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to load rulesets from path '{options.RulesetPath}': {e.Message}");
                return 1;
            }

            IEnumerable<Ruleset> rulesets = rulesetManager.GetAllRulesets().OrderBy(r => r.RulesetInfo.OnlineID);
            Dictionary<string, VersionEntry> versionEntries;
            if (options.CurrentVersionFile != null)
            {
                string currentJson = File.ReadAllText(options.CurrentVersionFile);
                versionEntries =
                    JsonConvert.DeserializeObject<Dictionary<string, VersionEntry>>(currentJson)
                    ?? new Dictionary<string, VersionEntry>();
            }
            else
            {
                versionEntries = new Dictionary<string, VersionEntry>();
            }

            foreach (Ruleset ruleset in rulesets)
            {
                string rulesetName = ruleset.RulesetInfo.ShortName;
                if (!versionEntries.TryGetValue(rulesetName, out VersionEntry? value))
                {
                    value = new VersionEntry();
                    versionEntries[rulesetName] = value;
                }

                UpdateVersionEntry(ruleset, value, options.Version);
            }

            string outputJson = JsonConvert.SerializeObject(versionEntries, Formatting.Indented);
            if (options.OutputPath != null)
            {
                File.WriteAllText(options.OutputPath, outputJson);
            }
            else
            {
                Console.WriteLine(outputJson);
            }

            return 0;
        }

        private static void UpdateVersionEntry(Ruleset ruleset, VersionEntry entry, string newVersion)
        {
            string md5 = GetFileMd5(GetRulesetAssembly(ruleset));
            entry.Versions[newVersion] = md5;
            if (VersionHelper.CompareVersionDates(newVersion, entry.LatestVersion) > 0)
            {
                entry.LatestVersion = newVersion;
            }
        }

        private static string GetFileMd5(string path)
        {
            using MD5 md5 = MD5.Create();
            using FileStream stream = File.OpenRead(path);
            byte[] hashBytes = md5.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private static string GetFileMd5(Assembly assembly)
        {
            string assemblyLocation = assembly.Location;
            return GetFileMd5(assemblyLocation);
        }

        private static Assembly GetRulesetAssembly(Ruleset ruleset) => ruleset.GetType().Assembly;
    }
}