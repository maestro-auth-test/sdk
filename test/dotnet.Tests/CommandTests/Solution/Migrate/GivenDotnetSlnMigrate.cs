﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Cli.Sln.Migrate.Tests
{
    public class GivenDotnetSlnMigrate : SdkTest
    {
        public GivenDotnetSlnMigrate(ITestOutputHelper log) : base(log) { }

        [Theory]
        [InlineData("sln")]
        [InlineData("solution")]
        public void WhenSlnFileIsValidShouldGenerateValidSlnxFile(string solutionCommand)
        {
            var projectDirectory = _testAssetsManager
                .CopyTestAsset("TestAppWithEmptySln")
                .WithSource()
                .Path;
            var slnFileName = Path.Combine(projectDirectory, "App.sln");
            var slnMigrateCommand = new DotnetCommand(Log)
                .WithWorkingDirectory(projectDirectory)
                .Execute(solutionCommand, "App.sln", "migrate");
            slnMigrateCommand.Should().Pass();

            var slnxFileName = Path.ChangeExtension(slnFileName, ".slnx");
            var slnxBuildCommand = new DotnetCommand(Log)
                .WithWorkingDirectory(projectDirectory)
                .Execute("build", slnxFileName);
            slnxBuildCommand.Should().ExitWith(0);
        }
    }
}
