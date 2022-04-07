using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using Nuke.Common.Tools.DotNet;
using System.IO.Compression;
using static Nuke.Common.IO.CompressionTasks;
using ICSharpCode.SharpZipLib.Tar;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using Nuke.Common.Tools.NuGet;

[CheckBuildProjectConfigurations]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Parameter("publish runtime")]
    readonly string Runtime = null;
    [Parameter("version suffix")]
    readonly string VersionSuffix = "";
    [Parameter("nuget api key")]
    readonly string ApiKey = null;
    [Parameter("nuget source")]
    readonly string PackageSource = "https://api.nuget.org/v3/index.json";

    AbsolutePath ArtifactDir = RootDirectory / "artifacts";

    [Solution] readonly Solution Solution;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetClean(cfg =>
            {
                return cfg.SetConfiguration(Configuration)
                    .SetRuntime(Runtime)
                    .SetProject(Solution);
            });
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(cfg => cfg.SetRuntime(Runtime)
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(cfg => cfg.SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .SetVersionSuffix(VersionSuffix)
                .SetRuntime(Runtime));
        });
    Target Test => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(cfg => cfg.SetConfiguration(Configuration)
                .AddLoggers("trx"));
        });
    Target Publish => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var projectFile = RootDirectory / "Cs2Mermaid" / "Cs2Mermaid.csproj";
            var outdir = !string.IsNullOrEmpty(Runtime) ? ArtifactDir / Configuration / Runtime : ArtifactDir / Configuration / "Any";
            DotNetTasks.DotNetPublish(cfg => cfg.SetConfiguration(Configuration)
                .SetProject(projectFile)
                .SetRuntime(Runtime)
                .SetOutput(outdir)
                .SetSelfContained(!string.IsNullOrEmpty(Runtime))
                .SetPublishSingleFile(!string.IsNullOrEmpty(Runtime))
                .SetPublishTrimmed(!string.IsNullOrEmpty(Runtime))
                .SetVersionSuffix(VersionSuffix)
                .SetProperty("LinkMode", "copyused")
                );
        });
    Target Pack => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var projectFile = RootDirectory / "Cs2Mermaid" / "Cs2Mermaid.csproj";
            var outdir = !string.IsNullOrEmpty(Runtime) ? ArtifactDir / Configuration / Runtime : ArtifactDir / Configuration / "Any";
            DotNetTasks.DotNetPack(cfg => cfg.SetConfiguration(Configuration)
                .SetProject(projectFile)
                .SetOutputDirectory(outdir)
                .SetVersionSuffix(VersionSuffix)
                );
        });
    Target ArchiveZip => _ => _
        .Requires(() => !string.IsNullOrEmpty(Runtime))
        .DependsOn(Publish)
        .Executes(() =>
        {
            var runtimedir = ArtifactDir / Configuration / Runtime;
            var archivedir = ArtifactDir / "archive" / Configuration / Runtime;
            var destdir = archivedir / "cs2mmd";
            var outFile = archivedir / $"cs2mmd-{Runtime}.zip";
            EnsureCleanDirectory(destdir);
            CopyDirectoryRecursively(runtimedir, destdir, DirectoryExistsPolicy.Merge);
            CompressZip(destdir, outFile, fileMode: System.IO.FileMode.Create);
        });
    Target ArchiveTgz => _ => _
        .Requires(() => !string.IsNullOrEmpty(Runtime))
        .DependsOn(Publish)
        .Executes(() =>
        {
            var runtimedir = ArtifactDir / Configuration / Runtime;
            var archivedir = ArtifactDir / "archive" / Configuration / Runtime;
            var destdir = archivedir / "cs2mmd";
            var outFile = archivedir / $"cs2mmd-{Runtime}.tgz";
            EnsureCleanDirectory(destdir);
            CopyDirectoryRecursively(runtimedir, destdir, DirectoryExistsPolicy.Merge);
            using var fstm = System.IO.File.Create(outFile);
            using (var zstm = new GZipStream(fstm, CompressionMode.Compress))
            using (var tstm = new TarOutputStream(zstm, System.Text.Encoding.UTF8))
            {
                foreach (var fi in new System.IO.DirectoryInfo(destdir).EnumerateFiles())
                {
                    var entry = TarEntry.CreateTarEntry("cs2mmd/" + fi.Name);
                    entry.ModTime = fi.LastWriteTime;
                    entry.GroupId = 0;
                    entry.UserId = 0;
                    entry.TarHeader.Mode = (fi.Name == "cs2mmd" || fi.Name == "cs2mmd.exe") ? 0x1ed : 0x1a4;
                    entry.Size = fi.Length;
                    tstm.PutNextEntry(entry);
                    using(var istm = fi.OpenRead())
                    {
                        istm.CopyTo(tstm);
                    }
                    tstm.CloseEntry();
                }
            }
        });
    Target Push => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            var targetFile = GlobFiles(ArtifactDir / Configuration / "Any", "*.nupkg").FirstOrDefault();
            if(targetFile == null)
            {
                throw new Exception("targetFile not found");
            }
            Serilog.Log.Information("targetFile is {0}", targetFile);
            DotNetTasks.DotNetNuGetPush(cfg => cfg.SetApiKey(ApiKey)
                .SetTargetPath(targetFile)
                .SetSource(PackageSource))
                ;
        });
}
