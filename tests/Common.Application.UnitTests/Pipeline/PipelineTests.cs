using Common.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Common.Application.UnitTests.Pipeline;

public class PipelineTests
{
    [Fact]
    public void AddCommonApplication_Should_NotThrow_WhenNoModuleHasHandlers()
    {
        var services = new ServiceCollection();

        Should.NotThrow(() => services.AddCommonApplication([]));
    }

    [Fact]
    public async Task Command_Should_ReturnValidationError_WithoutRunningHandler_WhenInvalid()
    {
        using ServiceProvider provider = BuildProvider();
        ICommandHandler<Ping, string> handler = provider.GetRequiredService<ICommandHandler<Ping, string>>();

        Result<string> result = await handler.Handle(new Ping(string.Empty), CancellationToken.None);

        result.Error.ShouldBeOfType<ValidationError>();
        provider.GetRequiredService<CallCounter>().Calls.ShouldBe(0);
    }

    [Fact]
    public async Task Command_Should_RunHandlerOnce_WhenValid()
    {
        using ServiceProvider provider = BuildProvider();
        ICommandHandler<Ping, string> handler = provider.GetRequiredService<ICommandHandler<Ping, string>>();

        Result<string> result = await handler.Handle(new Ping("hello"), CancellationToken.None);

        result.Value.ShouldBe("pong: hello");
        provider.GetRequiredService<CallCounter>().Calls.ShouldBe(1);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<CallCounter>();
        services.AddCommonApplication([typeof(PipelineTests).Assembly]);   // this assembly = a "module"

        return services.BuildServiceProvider();
    }
}

public sealed record Ping(string Message) : ICommand<string>;

public sealed class CallCounter
{
    public int Calls { get; set; }
}

internal sealed class PingCommandHandler(CallCounter counter) : ICommandHandler<Ping, string>
{
    public Task<Result<string>> Handle(Ping command, CancellationToken cancellationToken)
    {
        counter.Calls++;
        return Task.FromResult<Result<string>>($"pong: {command.Message}");
    }
}

internal sealed class PingValidator : AbstractValidator<Ping>
{
    public PingValidator()
    {
        RuleFor(c => c.Message).NotEmpty();
    }
}
