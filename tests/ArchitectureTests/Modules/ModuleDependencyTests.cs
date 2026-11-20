using System.Reflection;
using NetArchTest.Rules;
using Shouldly;

namespace ArchitectureTests.Modules;

public class ModuleDependencyTests : BaseTest
{
    [Fact]
    public void Modules_Should_NotDependOnOtherModulesInternals()
    {
        List<string> violations = [];

        foreach (string module in Modules)
        {
            string[] forbidden = [.. Modules
                .Where(other => other != module)
                .SelectMany(other => Internals.Select(layer => $"{other}.{layer}"))];

            TestResult result = Types.InAssemblies(ModuleAssemblies(module))
                .Should()
                .NotHaveDependencyOnAny(forbidden)
                .GetResult();

            violations.AddRange(result.FailingTypeNames ?? []);
        }

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void Common_Should_NotDependOnAnyModule()
    {
        Assembly[] common = [.. CommonProjects.Select(Assembly.Load)];

        TestResult result = Types.InAssemblies(common)
            .Should()
            .NotHaveDependencyOnAny(Modules)
            .GetResult();

        (result.FailingTypeNames ?? []).ShouldBeEmpty();
    }
}
