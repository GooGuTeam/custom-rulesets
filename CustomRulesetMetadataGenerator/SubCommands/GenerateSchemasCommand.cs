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

            IEnumerable<Ruleset> rulesets = rulesetManager.GetAllRulesets().OrderBy(r => r.RulesetInfo.OnlineID);

            NewtonsoftJsonSchemaGeneratorSettings settings = new()
            {
                FlattenInheritanceHierarchy = false, SchemaType = SchemaType.JsonSchema
            };
            JsonSchemaGenerator generator = new(settings);
            JsonSchema schema = new() { Title = "osu! Attribute models", };
            IEnumerable<Tuple<Type?, Type?, string>> types = rulesets.Select(GetAttributeTypesForRuleset).Distinct();

            JsonSchema perfBaseSchema = generator.Generate(typeof(PerformanceAttributes));
            perfBaseSchema.Title = nameof(PerformanceAttributes);
            perfBaseSchema.AllowAdditionalProperties = true;
            foreach (KeyValuePair<string, JsonSchemaProperty> prop in perfBaseSchema.Properties)
            {
                prop.Value.IsRequired = true;
            }

            schema.Definitions.Add(nameof(PerformanceAttributes), perfBaseSchema);

            JsonSchema diffBaseSchema = generator.Generate(typeof(DifficultyAttributes));
            diffBaseSchema.Title = nameof(DifficultyAttributes);
            diffBaseSchema.AllowAdditionalProperties = true;
            foreach (KeyValuePair<string, JsonSchemaProperty> prop in diffBaseSchema.Properties)
            {
                prop.Value.IsRequired = true;
            }

            schema.Definitions.Add(nameof(DifficultyAttributes), diffBaseSchema);

            foreach ((Type? performanceType, Type? difficultyType, string rulesetName) in types)
            {
                JsonSchema performanceSchema = GeneratePerformanceAttributesSchema(generator, rulesetName,
                    performanceType, ref perfBaseSchema);
                schema.Definitions.Add(performanceSchema.Title!, performanceSchema);

                JsonSchema difficultySchema = GenerateDifficultyAttributesSchema(generator, rulesetName,
                    difficultyType, ref diffBaseSchema);
                schema.Definitions.Add(difficultySchema.Title!, difficultySchema);
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

        private static JsonSchema GeneratePerformanceAttributesSchema(JsonSchemaGenerator generator, string rulesetName,
            Type? type, ref JsonSchema baseSchema)
        {
            if (type == null)
            {
                JsonSchema schema = generator.Generate(typeof(PerformanceAttributes));
                schema.Title = $"{Capitalize(rulesetName)}{nameof(PerformanceAttributes)}";
                schema.Properties.Clear();
                schema.AllowAdditionalProperties = true;
                schema.AllOf.Add(new JsonSchema() { Reference = baseSchema });
                return schema;
            }

            JsonSchema performanceSchema = generator.Generate(type);
            performanceSchema.Title = type.Name;
            performanceSchema.Definitions.Clear();
            foreach (JsonSchema allOfItem in performanceSchema.AllOf)
            {
                if (allOfItem.Reference != null)
                {
                    allOfItem.Reference = baseSchema;
                }
                else
                {
                    allOfItem.AllowAdditionalProperties = true;
                    foreach (KeyValuePair<string, JsonSchemaProperty> prop in allOfItem.Properties)
                    {
                        prop.Value.IsRequired = true;
                    }
                }
            }

            return performanceSchema;
        }

        private static JsonSchema GenerateDifficultyAttributesSchema(JsonSchemaGenerator generator, string rulesetName,
            Type? type, ref JsonSchema baseSchema)
        {
            if (type == null)
            {
                JsonSchema schema = generator.Generate(typeof(DifficultyAttributes));
                schema.Title = $"{Capitalize(rulesetName)}{nameof(DifficultyAttributes)}";
                schema.Properties.Clear();
                schema.AllowAdditionalProperties = true;
                schema.AllOf.Add(new JsonSchema() { Reference = baseSchema });
                return schema;
            }

            JsonSchema difficultySchema = generator.Generate(type);
            difficultySchema.Title = type.Name;
            difficultySchema.AllowAdditionalItems = true;
            difficultySchema.Definitions.Clear();
            foreach (JsonSchema allOfItem in difficultySchema.AllOf)
            {
                if (allOfItem.Reference != null)
                {
                    allOfItem.Reference = baseSchema;
                }
                else
                {
                    allOfItem.AllowAdditionalProperties = true;
                    foreach (KeyValuePair<string, JsonSchemaProperty> prop in allOfItem.Properties)
                    {
                        prop.Value.IsRequired = true;
                    }
                }
            }

            return difficultySchema;
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