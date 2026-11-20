using System.Reflection;

namespace ArchitectureTests;

public abstract class BaseTest
{
    protected static readonly string[] Modules = ["Users", "Todos"];
    protected static readonly string[] Internals = ["Domain", "Application", "Infrastructure"];
    protected static readonly string[] CommonProjects = ["SharedKernel", "Common.Application", "Common.Infrastructure"];

    protected static readonly DirectoryInfo SolutionRoot = FindSolutionRoot();

    protected static Assembly[] ModuleAssemblies(string module) =>
        [.. Internals.Append("Contracts").Select(layer => Assembly.Load($"{module}.{layer}"))];

    private static DirectoryInfo FindSolutionRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaskFlow.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new DirectoryNotFoundException("TaskFlow.slnx not found above the test output.");
    }
}
