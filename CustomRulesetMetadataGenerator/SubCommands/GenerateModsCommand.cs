// Copyright (c) 2025 GooGuTeam
// Licensed under the AGPL-3.0 Licence. See the LICENCE file in the repository root for full licence text.
// Some code from https://github.com/ppy/osu-tools/blob/master/PerformanceCalculator/Difficulty/ModsCommand.cs with copyright (c) ppy Pty Ltd <contact@ppy.sh>.

using CommandLine;
using Newtonsoft.Json;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using System.Diagnostics;
using System.Reflection;

namespace CustomRulesetGenerator.SubCommands
{
    [Verb("mods", HelpText = "Generate mods for all rulesets found in the specified path.")]
    public class GenerateModsOptions : BaseOptions
    {
        [Option("include-official", Default = true, HelpText = "Include official rulesets when generating mods.")]
        public bool IncludeOfficial { get; set; }
    }

    public class GenerateModsCommand(GenerateModsOptions options) : ISubCommand
    {
        public int Execute()
        {
            RulesetManager rulesetManager;
            try
            {
                rulesetManager = new RulesetManager(options.RulesetPath, options.IncludeOfficial);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to load rulesets from path '{options.RulesetPath}': {e.Message}");
                return 1;
            }

            IEnumerable<Ruleset> rulesets = rulesetManager.GetAllRulesets().OrderBy(r => r.RulesetInfo.OnlineID);
            string dumped = JsonConvert.SerializeObject(
                rulesets.Select(r => new
                {
                    Name = r.RulesetInfo.ShortName,
                    RulesetID = r.RulesetInfo.OnlineID,
                    Mods = GetDefinitionsForRuleset(r)
                }), Formatting.Indented);
            if (options.OutputPath != null)
            {
                File.WriteAllText(options.OutputPath, dumped);
            }
            else
            {
                Console.WriteLine(dumped);
            }

            return 0;
        }

        private static IEnumerable<dynamic> GetDefinitionsForRuleset(Ruleset ruleset)
        {
            IEnumerable<Mod> allMods = ruleset.CreateAllMods();

            IEnumerable<Mod> enumerable = allMods as Mod[] ?? allMods.ToArray();
            return enumerable.Select(mod => new
            {
                mod.Acronym,
                mod.Name,
                Description = mod.Description.ToString(),
                Type = mod.Type.ToString(),
                Settings = GetSettingsDefinitions(mod),
                IncompatibleMods = GetAllImplementations(mod.IncompatibleMods),
                mod.RequiresConfiguration,
                mod.UserPlayable,
                mod.ValidForMultiplayer,
                mod.ValidForFreestyleAsRequiredMod,
                mod.ValidForMultiplayerAsFreeMod,
                mod.AlwaysValidForSubmission,
            });

            IEnumerable<string> GetAllImplementations(Type[] incompatibleTypes)
            {
                foreach (Mod mod in enumerable)
                {
                    if (incompatibleTypes.Any(t => t.IsInstanceOfType(mod)))
                        yield return mod.Acronym;
                }
            }

            IEnumerable<dynamic> GetSettingsDefinitions(Mod mod)
            {
                IEnumerable<(SettingSourceAttribute, PropertyInfo)>
                    sourceProperties = mod.GetSettingsSourceProperties();

                foreach ((SettingSourceAttribute settingsSource, PropertyInfo propertyInfo) in sourceProperties)
                {
                    object? bindable = propertyInfo.GetValue(mod);

                    Debug.Assert(bindable != null);

                    object? underlyingValue = (object?)bindable.GetUnderlyingSettingValue();
                    Type? netType = underlyingValue?.GetType() ?? bindable.GetType().GetInterface("IBindable`1")
                        ?.GenericTypeArguments.FirstOrDefault();

                    yield return new
                    {
                        Name = propertyInfo.Name.ToSnakeCase(),
                        Type = GetJsonType(netType),
                        Label = settingsSource.Label.ToString(),
                        Description = settingsSource.Description.ToString(),
                    };
                }
            }
        }

        private static string GetJsonType(Type? netType)
        {
            if (netType == typeof(int))
                return "number";
            if (netType == typeof(double))
                return "number";
            if (netType == typeof(float))
                return "number";
            if (netType == typeof(int?))
                return "number";
            if (netType == typeof(double?))
                return "number";
            if (netType == typeof(float?))
                return "number";

            if (netType == typeof(bool))
                return "boolean";
            if (netType == typeof(bool?))
                return "boolean";

            if (netType == typeof(string))
                return "string";

            return netType?.IsEnum == true ? "string" : throw new ArgumentOutOfRangeException(nameof(netType));
        }
    }
}