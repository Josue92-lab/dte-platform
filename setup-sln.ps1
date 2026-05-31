$slnPath = "DtePlatform.sln"

$projects = @(
    @{ Name="DTE.Domain"; Path="src\DTE.Domain\DTE.Domain.csproj"; Guid=[guid]::NewGuid().ToString("B").ToUpper(); Type="src" },
    @{ Name="DTE.Application"; Path="src\DTE.Application\DTE.Application.csproj"; Guid=[guid]::NewGuid().ToString("B").ToUpper(); Type="src" },
    @{ Name="DTE.Infrastructure"; Path="src\DTE.Infrastructure\DTE.Infrastructure.csproj"; Guid=[guid]::NewGuid().ToString("B").ToUpper(); Type="src" },
    @{ Name="DTE.Api"; Path="src\DTE.Api\DTE.Api.csproj"; Guid=[guid]::NewGuid().ToString("B").ToUpper(); Type="src" },
    @{ Name="DTE.Domain.Tests"; Path="tests\DTE.Domain.Tests\DTE.Domain.Tests.csproj"; Guid=[guid]::NewGuid().ToString("B").ToUpper(); Type="tests" },
    @{ Name="DTE.Application.Tests"; Path="tests\DTE.Application.Tests\DTE.Application.Tests.csproj"; Guid=[guid]::NewGuid().ToString("B").ToUpper(); Type="tests" },
    @{ Name="DTE.Infrastructure.Tests"; Path="tests\DTE.Infrastructure.Tests\DTE.Infrastructure.Tests.csproj"; Guid=[guid]::NewGuid().ToString("B").ToUpper(); Type="tests" },
    @{ Name="DTE.Api.Tests"; Path="tests\DTE.Api.Tests\DTE.Api.Tests.csproj"; Guid=[guid]::NewGuid().ToString("B").ToUpper(); Type="tests" },
    @{ Name="DTE.Architecture.Tests"; Path="tests\DTE.Architecture.Tests\DTE.Architecture.Tests.csproj"; Guid=[guid]::NewGuid().ToString("B").ToUpper(); Type="tests" }
)

$srcFolderGuid = [guid]::NewGuid().ToString("B").ToUpper()
$testsFolderGuid = [guid]::NewGuid().ToString("B").ToUpper()
$solutionItemsFolderGuid = [guid]::NewGuid().ToString("B").ToUpper()

$csharpProjectTypeGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"
$folderProjectTypeGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}"

$slnContent = "Microsoft Visual Studio Solution File, Format Version 12.00`r`n# Visual Studio Version 17`r`nVisualStudioVersion = 17.0.31903.59`r`nMinimumVisualStudioVersion = 10.0.40219.1`r`n"

# Add Folders
$slnContent += "Project(`"$folderProjectTypeGuid`") = `"src`", `"src`", `"$srcFolderGuid`"`r`nEndProject`r`n"
$slnContent += "Project(`"$folderProjectTypeGuid`") = `"tests`", `"tests`", `"$testsFolderGuid`"`r`nEndProject`r`n"
$slnContent += "Project(`"$folderProjectTypeGuid`") = `"Solution Items`", `"Solution Items`", `"$solutionItemsFolderGuid`"`r`n"
$slnContent += "	ProjectSection(SolutionItems) = preProject`r`n"
$slnContent += "		.editorconfig = .editorconfig`r`n"
$slnContent += "		Directory.Build.props = Directory.Build.props`r`n"
$slnContent += "		Directory.Packages.props = Directory.Packages.props`r`n"
$slnContent += "		global.json = global.json`r`n"
$slnContent += "		NuGet.config = NuGet.config`r`n"
$slnContent += "		README.md = README.md`r`n"
$slnContent += "	EndProjectSection`r`n"
$slnContent += "EndProject`r`n"

# Add Projects
foreach ($p in $projects) {
    $slnContent += "Project(`"$csharpProjectTypeGuid`") = `"$($p.Name)`", `"$($p.Path)`", `"$($p.Guid)`"`r`nEndProject`r`n"
}

$slnContent += "Global`r`n"
$slnContent += "	GlobalSection(SolutionConfigurationPlatforms) = preSolution`r`n"
$slnContent += "		Debug|Any CPU = Debug|Any CPU`r`n"
$slnContent += "		Release|Any CPU = Release|Any CPU`r`n"
$slnContent += "	EndGlobalSection`r`n"

$slnContent += "	GlobalSection(ProjectConfigurationPlatforms) = postSolution`r`n"
foreach ($p in $projects) {
    $slnContent += "		$($p.Guid).Debug|Any CPU.ActiveCfg = Debug|Any CPU`r`n"
    $slnContent += "		$($p.Guid).Debug|Any CPU.Build.0 = Debug|Any CPU`r`n"
    $slnContent += "		$($p.Guid).Release|Any CPU.ActiveCfg = Release|Any CPU`r`n"
    $slnContent += "		$($p.Guid).Release|Any CPU.Build.0 = Release|Any CPU`r`n"
}
$slnContent += "	EndGlobalSection`r`n"

$slnContent += "	GlobalSection(NestedProjects) = preSolution`r`n"
foreach ($p in $projects) {
    if ($p.Type -eq "src") {
        $slnContent += "		$($p.Guid) = $srcFolderGuid`r`n"
    } else {
        $slnContent += "		$($p.Guid) = $testsFolderGuid`r`n"
    }
}
$slnContent += "	EndGlobalSection`r`n"
$slnContent += "EndGlobal`r`n"

Set-Content -Path $slnPath -Value $slnContent

# Create basic csproj files
$domainPath = "src\DTE.Domain\DTE.Domain.csproj"
$appPath = "src\DTE.Application\DTE.Application.csproj"
$infraPath = "src\DTE.Infrastructure\DTE.Infrastructure.csproj"
$apiPath = "src\DTE.Api\DTE.Api.csproj"

Set-Content $domainPath "<Project Sdk=`"Microsoft.NET.Sdk`">`r`n</Project>"

Set-Content $appPath @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\DTE.Domain\DTE.Domain.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="MediatR" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
  </ItemGroup>
</Project>
"@

Set-Content $infraPath @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\DTE.Application\DTE.Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
    <PackageReference Include="Hangfire.PostgreSql" />
    <PackageReference Include="Hangfire.AspNetCore" />
  </ItemGroup>
</Project>
"@

Set-Content $apiPath @"
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <ProjectReference Include="..\DTE.Application\DTE.Application.csproj" />
    <ProjectReference Include="..\DTE.Infrastructure\DTE.Infrastructure.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Serilog.AspNetCore" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
</Project>
"@

$testBase = @"
  <ItemGroup>
    <PackageReference Include="xUnit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>
"@

Set-Content "tests\DTE.Domain.Tests\DTE.Domain.Tests.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\src\DTE.Domain\DTE.Domain.csproj" />
  </ItemGroup>
$testBase
</Project>
"@

Set-Content "tests\DTE.Application.Tests\DTE.Application.Tests.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\src\DTE.Application\DTE.Application.csproj" />
  </ItemGroup>
$testBase
  <ItemGroup>
    <PackageReference Include="NSubstitute" />
  </ItemGroup>
</Project>
"@

Set-Content "tests\DTE.Infrastructure.Tests\DTE.Infrastructure.Tests.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\src\DTE.Infrastructure\DTE.Infrastructure.csproj" />
  </ItemGroup>
$testBase
  <ItemGroup>
    <PackageReference Include="Testcontainers.PostgreSql" />
  </ItemGroup>
</Project>
"@

Set-Content "tests\DTE.Api.Tests\DTE.Api.Tests.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\src\DTE.Api\DTE.Api.csproj" />
  </ItemGroup>
$testBase
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
  </ItemGroup>
</Project>
"@

Set-Content "tests\DTE.Architecture.Tests\DTE.Architecture.Tests.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\src\DTE.Domain\DTE.Domain.csproj" />
    <ProjectReference Include="..\..\src\DTE.Application\DTE.Application.csproj" />
    <ProjectReference Include="..\..\src\DTE.Infrastructure\DTE.Infrastructure.csproj" />
    <ProjectReference Include="..\..\src\DTE.Api\DTE.Api.csproj" />
  </ItemGroup>
$testBase
  <ItemGroup>
    <PackageReference Include="NetArchTest.Rules" />
  </ItemGroup>
</Project>
"@
