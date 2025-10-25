// Copyright (c) 2025 GooGuTeam
// Licensed under the AGPL-3.0 Licence. See the LICENCE file in the repository root for full licence text.

using CommandLine;
using CommandLine.Text;
using CustomRulesetGenerator.SubCommands;
using System.Reflection;

namespace CustomRulesetGenerator
{
    public class BaseOptions
    {
        [Value(0, MetaName = "path", Required = true, HelpText = "RulesetPath to load rulesets from.")]
        public required string RulesetPath { get; set; }

        [Option('o', "output", Default = null,
            HelpText = "Output to a specific file. If not specified, output to console.")]
        public string? OutputPath { get; set; }
    }

    internal static class Program
    {
        private static int Main(string[] args)
        {
            Type[] types = LoadVerbs();
            Parser parser = new CommandLine.Parser(with => with.HelpWriter = null);
            ParserResult<object>? parsedResult = parser.ParseArguments(args, types);
            return
                parsedResult.MapResult(
                    (GenerateModsOptions opts) => new GenerateModsCommand(opts).Execute(),
                    (GenerateVersionOptions opts) => new GenerateVersionCommand(opts).Execute(),
                    (GenerateSchemasOptions opts) => new GenerateSchemasCommand(opts).Execute(),
                    errs => DisplayHelp(parsedResult));
        }

        private static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
        }


        private static int DisplayHelp<T>(ParserResult<T> result)
        {
            HelpText? helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = "CustomRulesetMetadataGenerator";
                h.Copyright = "Copyright (c) 2025 GooGuTeam";
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
            return 1;
        }
    }
}