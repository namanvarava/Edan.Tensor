using System.Xml.Linq;
using Xunit;

namespace EDAN.Tensor.SharedKernel.Tests;

/// <summary>소스 프로젝트의 참조 그래프가 모듈과 계층 경계를 유지하는지 검증합니다.</summary>
public sealed class ArchitectureDependencyTests
{
    [Fact]
    public void SharedKernelHasNoProjectOrPackageDependencies()
    {
        var project = Projects().Single(x => x.Name == "EDAN.Tensor.SharedKernel");
        Assert.Empty(project.Document.Descendants("ProjectReference"));
        Assert.Empty(project.Document.Descendants("PackageReference"));
    }

    [Fact]
    public void DomainAndApplicationReferencesStayWithinApprovedLayers()
    {
        foreach (var project in Projects())
        {
            var layer = project.Name.Split('.').Last();
            if (layer is not ("Domain" or "Application")) continue;
            var modulePrefix = project.Name[..project.Name.LastIndexOf('.')];
            foreach (var reference in project.Document.Descendants("ProjectReference"))
            {
                var target = ReferenceName(reference);
                var allowed = target == "EDAN.Tensor.SharedKernel"
                    || layer == "Application" && (target == modulePrefix + ".Domain"
                        || target == modulePrefix + ".Contracts");
                Assert.True(allowed, $"{project.Name} → {target}: 허용되지 않은 계층 의존성");
            }

            var packages = project.Document.Descendants("PackageReference")
                .Select(x => (string?)x.Attribute("Include"));
            Assert.All(packages, package => Assert.True(
                layer == "Application" && package == "Microsoft.Extensions.DependencyInjection.Abstractions",
                $"{project.Name}: 허용되지 않은 패키지 {package}"));
        }
    }

    [Fact]
    public void BusinessModulesDoNotReferenceHostsOrEachOthersInternals()
    {
        foreach (var project in Projects().Where(x => !x.RelativePath.StartsWith("Hosts/")))
        {
            foreach (var reference in project.Document.Descendants("ProjectReference"))
            {
                var target = ReferenceName(reference);
                Assert.DoesNotContain(".Host", target);
                if (project.Name.StartsWith("EDAN.Tensor.Commercial."))
                    Assert.False(target.StartsWith("EDAN.Tensor.Licensing.")
                        && !target.EndsWith(".Contracts"), $"{project.Name} → {target}");
                if (project.Name.StartsWith("EDAN.Tensor.Licensing."))
                    Assert.False(target.StartsWith("EDAN.Tensor.Commercial.")
                        && !target.EndsWith(".Contracts"), $"{project.Name} → {target}");
            }
        }
    }

    private static string ReferenceName(XElement reference) =>
        Path.GetFileNameWithoutExtension(((string)reference.Attribute("Include")!).Replace('\\', '/'));

    private static IReadOnlyList<Project> Projects()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "EDAN.Tensor.sln")))
            root = root.Parent;
        Assert.NotNull(root);
        var source = Path.Combine(root.FullName, "src");
        return Directory.GetFiles(source, "*.csproj", SearchOption.AllDirectories)
            .Select(path => new Project(Path.GetFileNameWithoutExtension(path),
                Path.GetRelativePath(source, path).Replace('\\', '/'), XDocument.Load(path)))
            .ToArray();
    }

    private sealed record Project(string Name, string RelativePath, XDocument Document);
}
