// Copyright (c) 2025 GooGuTeam
// Licensed under the AGPL-3.0 Licence. See the LICENCE file in the repository root for full licence text.

using CommandLine;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.NewtonsoftJson.Generation;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using System.Reflection;

namespace CustomRulesetGenerator.SubCommands
{
    [Verb("schemas",
        HelpText = "Generate PerformanceAttribute & DifficultyAttribute for all rulesets found in the specified path.")]
    public class GenerateSchemasOptions : BaseOptions
    {
        [Option("include-official", Default = true, HelpText = "Include official rulesets when generating schemas.")]
        public bool IncludeOfficial { get; set; }
    }

    public class GenerateSchemasCommand(GenerateSchemasOptions options) : ISubCommand
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

            IEnumerable<Ruleset> rulesets = rulesetManager.GetAllRulesets();

            var settings = new NewtonsoftJsonSchemaGeneratorSettings();
            var generator = new JsonSchemaGenerator(settings);
            JsonSchema schema = new() { Title = "osu! Attribute models", Type = JsonObjectType.Object };
            IEnumerable<Tuple<Type?, Type?, string>> types = rulesets.Select(GetAttributeTypesForRuleset).Distinct();


            foreach ((Type? performanceType, Type? difficultyType, string rulesetName) in types)
            {
                Type type;
                string name;
                if (performanceType != null)
                {
                    type = performanceType;
                    name = performanceType.Name;
                }
                else
                {
                    type = typeof(PerformanceAttributes);
                    name = $"{Capitalize(rulesetName)}{nameof(PerformanceAttributes)}";
                }

                JsonSchema performanceSchema = generator.Generate(type);
                performanceSchema.Title = name;
                performanceSchema.AllowAdditionalProperties = true;
                schema.Definitions.Add(name, performanceSchema);

                if (difficultyType != null)
                {
                    type = difficultyType;
                    name = difficultyType.Name;
                }
                else
                {
                    type = typeof(DifficultyAttributes);
                    name = $"{Capitalize(rulesetName)}{nameof(DifficultyAttributes)}";
                }

                JsonSchema difficultySchema = generator.Generate(type);
                difficultySchema.Title = name;
                difficultySchema.AllowAdditionalProperties = true;
                schema.Definitions.Add(name, difficultySchema);
            }

            string json = schema.ToJson();

            if (options.OutputPath != null)
            {
                File.WriteAllText(options.OutputPath, json);
            }
            else
            {
                Console.WriteLine(json);
            }

            return 0;
        }

        private static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return char.ToUpper(s[0]) + s[1..];
        }

        private static Tuple<Type?, Type?, string> GetAttributeTypesForRuleset(Ruleset ruleset)
        {
            Type? performanceType = null;
            Type? difficultyType = null;

            foreach (Type type in Assembly.GetAssembly(ruleset.GetType())!.GetTypes())
            {
                if (type.IsSubclassOf(typeof(PerformanceAttributes)))
                    performanceType = type;
                else if (type.IsSubclassOf(typeof(DifficultyAttributes)))
                    difficultyType = type;
            }

            return Tuple.Create(performanceType, difficultyType, ruleset.ShortName);
        }
    }
}