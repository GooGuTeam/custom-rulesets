// Copyright (c) 2025 GooGuTeam
// Licensed under the AGPL-3.0 Licence. See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using System.Reflection;

namespace CustomRulesetGenerator
{
    public class RulesetManager
    {
        private const string RulesetLibraryPrefix = "osu.Game.Rulesets";

        private readonly Dictionary<string, Ruleset> _rulesets = new();
        private readonly Dictionary<int, Ruleset> _rulesetsById = new();

        public RulesetManager(string rulesetPath, bool includeOfficial = true)
        {
            if (includeOfficial)
                LoadOfficialRulesets();
            LoadFromDisk(rulesetPath);
        }

        private void AddRuleset(Ruleset ruleset)
        {
            if (!_rulesets.TryAdd(ruleset.ShortName, ruleset))
            {
                Console.Error.WriteLine($"Ruleset with short name {ruleset.ShortName} already exists, skipping.");
                return;
            }

            if (ruleset is not ILegacyRuleset legacyRuleset)
            {
                return;
            }

            if (!_rulesetsById.TryAdd(legacyRuleset.LegacyID, ruleset))
            {
                Console.Error.WriteLine($"Ruleset with ID {legacyRuleset.LegacyID} already exists, skipping.");
            }
        }

        private void LoadOfficialRulesets()
        {
            foreach (Ruleset ruleset in (List<Ruleset>)
                     [new OsuRuleset(), new TaikoRuleset(), new CatchRuleset(), new ManiaRuleset()])
            {
                AddRuleset(ruleset);
            }

            _rulesets["catch"] = _rulesets["fruits"];
        }

        private void LoadFromDisk(string rulesetPath)
        {
            if (!Directory.Exists(rulesetPath))
            {
                return;
            }

            string[] rulesets = Directory.GetFiles(rulesetPath, $"{RulesetLibraryPrefix}.*.dll");

            foreach (string ruleset in rulesets.Where(f => !f.Contains(@"Tests")))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(ruleset);
                    Type? rulesetType = assembly.GetTypes()
                        .FirstOrDefault(t => t.IsSubclassOf(typeof(Ruleset)) && !t.IsAbstract);

                    if (rulesetType == null)
                    {
                        continue;
                    }

                    Ruleset instance = (Ruleset)Activator.CreateInstance(rulesetType)!;
                    Console.Error.WriteLine($"Loading ruleset {ruleset}");
                    AddRuleset(instance);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to load ruleset from {ruleset}: {ex}");
                }
            }
        }

        public Ruleset GetRuleset(int rulesetId)
        {
            return _rulesetsById.TryGetValue(rulesetId, out Ruleset? ruleset)
                ? ruleset
                : throw new ArgumentException("Invalid ruleset ID provided.");
        }

        public Ruleset GetRuleset(string shortName)
        {
            return _rulesets.TryGetValue(shortName, out Ruleset? ruleset)
                ? ruleset
                : throw new ArgumentException("Invalid ruleset name provided.");
        }
        
        public IEnumerable<Ruleset> GetAllRulesets()
        {
            return _rulesets.Values;
        }
    }
}