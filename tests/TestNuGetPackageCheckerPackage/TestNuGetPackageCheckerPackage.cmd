echo OFF

SET NUGET_PACKAGES=%~dp0packages

dotnet build %~dp0\TestNuGetPackageCheckerPackage.proj /nodeReuse:false -v:Normal && (echo DotNet build package test successful) || (exit 1)

dotnet build %~dp0\TestNuGetPackageCheckerPackage.proj /p:GenerateBuildFailure=true -v:Normal /nodeReuse:false && (exit 1) || (echo Successfully generated expected build failure)

dotnet build %~dp0\TestNuGetPackageCheckerPackage.proj /p:GenerateForbiddenPackageBuildFailure=true -v:Normal /nodeReuse:false && (exit 1) || (echo Successfully generated expected build failure)

REM Bamboo doesn't currently have a compatible version of msbuild available
REM "C:\ProgramData\IDE\Visual Studio 2019\MSBuild\Current\Bin\amd64\MSBuild.exe" %~dp0\TestNuGetPackageCheckerPackage.proj /nodeReuse:false -v:Normal && (echo MsBuild build package test successful) || (exit 1)

exit 0
