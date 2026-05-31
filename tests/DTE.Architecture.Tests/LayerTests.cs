using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace DTE.Architecture.Tests;

public class LayerTests
{
    private const string DomainNamespace = "DTE.Domain";
    private const string ApplicationNamespace = "DTE.Application";
    private const string InfrastructureNamespace = "DTE.Infrastructure";
    private const string ApiNamespace = "DTE.Api";

    [Fact]
    public void Domain_Should_Not_Have_Dependencies_On_Other_Layers()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Have_Dependencies_On_Infrastructure_Or_Api()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_Have_Dependency_On_Api()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
