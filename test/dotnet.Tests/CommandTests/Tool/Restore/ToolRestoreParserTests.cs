// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.Commands.Tool;
using Microsoft.DotNet.Cli.Commands.Tool.Restore;
using Parser = Microsoft.DotNet.Cli.Parser;

namespace Microsoft.DotNet.Tests.ParserTests
{
    public class ToolRestoreParserTests
    {
        private readonly ITestOutputHelper output;

        public ToolRestoreParserTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ToolRestoreParserCanGetManifestFilePath()
        {
            var result = Parser.Instance.Parse("dotnet tool restore --tool-manifest folder/my-manifest.format");

            result.GetValue<string>(ToolRestoreCommandParser.ToolManifestOption).Should().Be("folder/my-manifest.format");
        }

        [Fact]
        public void ToolRestoreParserCanGetFollowingArguments()
        {
            var result =
                Parser.Instance.Parse(
                    @"dotnet tool restore --configfile C:\TestAssetLocalNugetFeed");

            result.GetValue<string>(ToolRestoreCommandParser.ConfigOption).Should().Be(@"C:\TestAssetLocalNugetFeed");
        }

        [Fact]
        public void ToolRestoreParserCanParseSourceOption()
        {
            const string expectedSourceValue = "TestSourceValue";

            var result =
                Parser.Instance.Parse($"dotnet tool restore --add-source {expectedSourceValue}");

            result.GetValue<string[]>(ToolRestoreCommandParser.AddSourceOption).First().Should().Be(expectedSourceValue);
        }

        [Fact]
        public void ToolRestoreParserCanParseMultipleSourceOption()
        {
            const string expectedSourceValue1 = "TestSourceValue1";
            const string expectedSourceValue2 = "TestSourceValue2";

            var result =
                Parser.Instance.Parse(
                    $"dotnet tool restore " +
                    $"--add-source {expectedSourceValue1} " +
                    $"--add-source {expectedSourceValue2}");

            result.GetValue<string[]>(ToolRestoreCommandParser.AddSourceOption)[0].Should().Be(expectedSourceValue1);
            result.GetValue<string[]>(ToolRestoreCommandParser.AddSourceOption)[1].Should().Be(expectedSourceValue2);
        }

        [Fact]
        public void ToolRestoreParserCanParseVerbosityOption()
        {
            const string expectedVerbosityLevel = "diag";

            var result = Parser.Instance.Parse($"dotnet tool restore --verbosity {expectedVerbosityLevel}");

            Enum.GetName(result.GetValue<VerbosityOptions>(ToolRestoreCommandParser.VerbosityOption)).Should().Be(expectedVerbosityLevel);
        }

        [Fact]
        public void ToolRestoreParserCanParseNoCacheOption()
        {
            var result =
                Parser.Instance.Parse(@"dotnet tool restore --no-cache");

            result.GetValue(ToolCommandRestorePassThroughOptions.NoCacheOption).Should().BeTrue();
        }

        [Fact]
        public void ToolRestoreParserCanParseNoHttpCacheOption()
        {
            var result =
                Parser.Instance.Parse(@"dotnet tool restore --no-http-cache");

            result.GetValue(ToolCommandRestorePassThroughOptions.NoHttpCacheOption).Should().BeTrue();
        }

        [Fact]
        public void ToolRestoreParserCanParseIgnoreFailedSourcesOption()
        {
            var result =
                Parser.Instance.Parse(@"dotnet tool restore --ignore-failed-sources");

            result.GetValue(ToolCommandRestorePassThroughOptions.IgnoreFailedSourcesOption).Should().BeTrue();
        }

        [Fact]
        public void ToolRestoreParserCanParseDisableParallelOption()
        {
            var result =
                Parser.Instance.Parse(@"dotnet tool restore --disable-parallel");

            result.GetValue(ToolCommandRestorePassThroughOptions.DisableParallelOption).Should().BeTrue();
        }

        [Fact]
        public void ToolRestoreParserCanParseInteractiveRestoreOption()
        {
            var result =
                Parser.Instance.Parse(@"dotnet tool restore --interactive");

            result.GetValue(ToolCommandRestorePassThroughOptions.InteractiveRestoreOption).Should().BeTrue();
        }
    }
}
