using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace DTE.Architecture.Tests;

public class LayerTests
{
    private const string _domainNamespace = "DTE.Domain";
    private const string _applicationNamespace = "DTE.Application";
    private const string _infrastructureNamespace = "DTE.Infrastructure";
    private const string _apiNamespace = "DTE.Api";

    [Fact]
    public void Domain_Should_Not_Have_Dependencies_On_Other_Layers()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(_domainNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(_applicationNamespace, _infrastructureNamespace, _apiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Have_Dependencies_On_Infrastructure_Or_Api()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(_applicationNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(_infrastructureNamespace, _apiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_Have_Dependency_On_Api()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(_infrastructureNamespace)
            .ShouldNot()
            .HaveDependencyOn(_apiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
