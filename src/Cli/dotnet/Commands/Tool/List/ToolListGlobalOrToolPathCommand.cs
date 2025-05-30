// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using Microsoft.DotNet.Cli.ToolPackage;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Cli.Utils.Extensions;
using Microsoft.Extensions.EnvironmentAbstractions;

namespace Microsoft.DotNet.Cli.Commands.Tool.List;

internal delegate IToolPackageStoreQuery CreateToolPackageStore(DirectoryPath? nonGlobalLocation = null);

internal class ToolListGlobalOrToolPathCommand(
    ParseResult result,
    CreateToolPackageStore createToolPackageStore = null,
    IReporter reporter = null) : CommandBase(result)
{
    public const string CommandDelimiter = ", ";
    private readonly IReporter _reporter = reporter ?? Reporter.Output;
    private readonly IReporter _errorReporter = reporter ?? Reporter.Error;
    private readonly CreateToolPackageStore _createToolPackageStore = createToolPackageStore ?? ToolPackageFactory.CreateToolPackageStoreQuery;

    public override int Execute()
    {
        var toolPathOption = _parseResult.GetValue(ToolListCommandParser.ToolPathOption);
        var packageIdArgument = _parseResult.GetValue(ToolListCommandParser.PackageIdArgument);

        PackageId? packageId = null;
        if (!string.IsNullOrWhiteSpace(packageIdArgument))
        {
            packageId = new PackageId(packageIdArgument);
        }

        DirectoryPath? toolPath = null;
        if (!string.IsNullOrWhiteSpace(toolPathOption))
        {
            if (!Directory.Exists(toolPathOption))
            {
                throw new GracefulException(
                    string.Format(
                        CliCommandStrings.ToolListInvalidToolPathOption,
                        toolPathOption));
            }

            toolPath = new DirectoryPath(toolPathOption);
        }

        var packageEnumerable = GetPackages(toolPath, packageId);

        var formatValue = _parseResult.GetValue(ToolListCommandParser.ToolListFormatOption);
        if (formatValue is ToolListOutputFormat.json)
        {
            PrintJson(packageEnumerable);
        }
        else
        {
            PrintTable(packageEnumerable);
        }

        if (packageId.HasValue && !packageEnumerable.Any())
        {
            // return 1 if target package was not found
            return 1;
        }
        return 0;
    }

    public IEnumerable<IToolPackage> GetPackages(DirectoryPath? toolPath, PackageId? packageId)
    {
        return [.. _createToolPackageStore(toolPath).EnumeratePackages()
            .Where((p) => PackageHasCommand(p) && PackageIdMatches(p, packageId))
            .OrderBy(p => p.Id)];
    }

    internal static bool PackageIdMatches(IToolPackage package, PackageId? packageId)
    {
        return !packageId.HasValue || package.Id.Equals(packageId);
    }

    private bool PackageHasCommand(IToolPackage package)
    {
        try
        {
            // Attempt to read the command
            // If it fails, print a warning and treat as no commands
            return package.Command is not null;
        }
        catch (Exception ex) when (ex is ToolConfigurationException)
        {
            _errorReporter.WriteLine(
                string.Format(
                    CliCommandStrings.ToolListInvalidPackageWarning,
                    package.Id,
                    ex.Message).Yellow());
            return false;
        }
    }

    private void PrintTable(IEnumerable<IToolPackage> packageEnumerable)
    {
        var table = new PrintableTable<IToolPackage>();

        table.AddColumn(
            CliCommandStrings.ToolListPackageIdColumn,
            p => p.Id.ToString());
        table.AddColumn(
            CliCommandStrings.ToolListVersionColumn,
            p => p.Version.ToNormalizedString());
        table.AddColumn(
            CliCommandStrings.ToolListCommandsColumn,
            p => p.Command.Name.ToString());

        table.PrintRows(packageEnumerable, l => _reporter.WriteLine(l));
    }

    private void PrintJson(IEnumerable<IToolPackage> packageEnumerable)
    {
        var jsonData = new VersionedDataContract<ToolListJsonContract[]>()
        {
            Data = [.. packageEnumerable.Select(p => new ToolListJsonContract
            {
                PackageId = p.Id.ToString(),
                Version = p.Version.ToNormalizedString(),
                Commands = [p.Command.Name.Value]
            })]
        };
        var jsonText = System.Text.Json.JsonSerializer.Serialize(jsonData, JsonHelper.NoEscapeSerializerOptions);
        _reporter.WriteLine(jsonText);
    }
}
