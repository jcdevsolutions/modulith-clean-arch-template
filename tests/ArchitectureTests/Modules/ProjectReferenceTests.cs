using System.Xml.Linq;
using Shouldly;

namespace ArchitectureTests.Modules;

public class ProjectReferenceTests : BaseTest
{
    [Fact]
    public void Modules_Should_ReferenceOtherModulesOnlyThroughContracts()
    {
        List<string> violations = [];

        foreach (FileInfo csproj in SolutionRoot.EnumerateFiles("*.csproj", SearchOption.AllDirectories))
        {
            string from = Path.GetFileNameWithoutExtension(csproj.Name);
            string fromModule = from.Split('.')[0];

            if (!Modules.Contains(fromModule))
            {
                continue;
            }

            foreach (string to in ProjectReferences(csproj))
            {
                string toModule = to.Split('.')[0];

                bool crossModuleInternal = Modules.Contains(toModule)
                    && toModule != fromModule
                    && !to.EndsWith(".Contracts", StringComparison.Ordinal);

                bool contractsLeak = from.EndsWith(".Contracts", StringComparison.Ordinal)
                    && Modules.Contains(toModule);

                if (crossModuleInternal || contractsLeak)
                {
                    violations.Add($"{from} -> {to}");
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    private static IEnumerable<string> ProjectReferences(FileInfo csproj) =>
        XDocument.Load(csproj.FullName)
            .Descendants("ProjectReference")
            .Select(reference => (string?)reference.Attribute("Include") ?? string.Empty)
            .Select(include => Path.GetFileNameWithoutExtension(include.Replace('\\', '/')));
}
