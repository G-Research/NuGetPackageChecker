using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace NuGetPackageChecker.Tests
{
    public class PackageReferenceValidatorTests
    {
        private readonly ITestOutputHelper _output;

        public PackageReferenceValidatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("Bob", "Unknown")]
        [InlineData("netcoreapp2.1", ".NETCoreApp,Version=v2.1")]
        [InlineData("netcoreapp3.1", ".NETCoreApp,Version=v3.1")]
        [InlineData("netstandard2.0", ".NETStandard,Version=v2.0")]
        [InlineData("netstandard2.1", ".NETStandard,Version=v2.1")]
        [InlineData("net471", ".NETFramework,Version=v4.7.1")]
        [InlineData("net472", ".NETFramework,Version=v4.7.2")]
        [InlineData("net5.0", "net5.0")]
        [InlineData("net5", "net5.0")]
        [InlineData("net5-windows", "net5.0-windows7.0")]
        [InlineData("net5.0-windows", "net5.0-windows7.0")]
        [InlineData("net6.0", "net6.0")]
        [InlineData("net6.0-windows", "net6.0-windows7.0")]
        [InlineData("net6.0-windows10.0", "net6.0-windows10.0")]
        [InlineData("net7.0", "net7.0")]
        public void GetFrameworkCorrectlyDeterminesFrameworkString(string framework, string expectedFrameworkIdentifier)
        {
            var packageReferenceValidator = new PackageReferenceValidator()
            {
                TargetFramework = framework,
                BuildEngine = new FakeBuildEngine(_output)
            };

            packageReferenceValidator.GetFrameworkName(framework).Should().Be(expectedFrameworkIdentifier);
        }

        [Fact]
        public void CorrectlyFiltersPackage()
        {
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var projectAssetsFile = Path.Combine(testDirectory, "test-project.assets.json");

            var packageReferenceValidator = new PackageReferenceValidator()
            {
                TargetFramework = "netcoreapp3.1",
                BuildEngine = new FakeBuildEngine(_output),
                ProjectAssetsFile = projectAssetsFile,
                ProjectFile = "Bob",
                RequiredPackages = new Microsoft.Build.Framework.ITaskItem[]
                {
                    new TaskItem("NerdBank.GitVersioning",
                        new Dictionary<string,string>
                        {
                            { "Version", "3.26.0" },
                            { "Filter", "Bob.Test" }
                        })
                }
            };

            packageReferenceValidator.Execute().Should().BeTrue("Required package doesn't match filter");
        }

        [Fact]
        public void CorrectlyValidatesRequiredForPackage()
        {
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var projectAssetsFile = Path.Combine(testDirectory, "test-project.assets.json");

            var packageReferenceValidator = new PackageReferenceValidator()
            {
                TargetFramework = "netcoreapp3.1",
                BuildEngine = new FakeBuildEngine(_output),
                ProjectAssetsFile = projectAssetsFile,
                ProjectFile = "Bob",
                RequiredPackages = new Microsoft.Build.Framework.ITaskItem[]
                {
                    new TaskItem("coverlet.collector",
                        new Dictionary<string,string>
                        {
                            { "Version", "1.3.0" },
                            { "RequiredFor", "Microsoft.Net.Test.SDK" }
                        })
                }
            };

            packageReferenceValidator.Execute().Should().BeTrue("Required coverlet.collector package not present.");
        }

        [Fact]
        public void MissingRequiredForPackageFailsValidation()
        {
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var projectAssetsFile = Path.Combine(testDirectory, "test-project.assets.json");

            var packageReferenceValidator = new PackageReferenceValidator()
            {
                TargetFramework = "netcoreapp3.1",
                BuildEngine = new FakeBuildEngine(_output),
                ProjectAssetsFile = projectAssetsFile,
                ProjectFile = "Bob",
                RequiredPackages = new Microsoft.Build.Framework.ITaskItem[]
                {
                    new TaskItem("coverlet.msbuild",
                        new Dictionary<string,string>
                        {
                            { "Version", "1.3.0" },
                            { "RequiredFor", "Microsoft.Net.Test.SDK" }
                        })
                }
            };

            packageReferenceValidator.Execute().Should().BeFalse("Required package for Test reference is not present");
        }

        [Fact]
        public void CorrectlyValidatesPackageVersion()
        {
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var projectAssetsFile = Path.Combine(testDirectory, "test-project.assets.json");

            var packageReferenceValidator = new PackageReferenceValidator()
            {
                TargetFramework = "netcoreapp3.1",
                BuildEngine = new FakeBuildEngine(_output),
                ProjectAssetsFile = projectAssetsFile,
                ProjectFile = "Bob",
                RequiredPackages = new Microsoft.Build.Framework.ITaskItem[]
                {
                    new TaskItem("NerdBank.GitVersioning",
                        new Dictionary<string,string>
                        {
                            { "Version", "3.26.0"                            },
                            { "Filter", "Bob" }
                        })
                }
            };

            packageReferenceValidator.Execute().Should().BeFalse();
        }

        [Fact]
        public void PackageWithNoVersionPassesValidation()
        {
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var projectAssetsFile = Path.Combine(testDirectory, "test-project.assets.json");

            var packageReferenceValidator = new PackageReferenceValidator()
            {
                TargetFramework = "netcoreapp3.1",
                BuildEngine = new FakeBuildEngine(_output),
                ProjectAssetsFile = projectAssetsFile,
                ProjectFile = "Bob",
                RequiredPackages = new Microsoft.Build.Framework.ITaskItem[]
                {
                    new TaskItem("NerdBank.GitVersioning",
                        new Dictionary<string,string>
                        {
                            { "TargetFramework", "netcoreapp3" }
                        })
                }
            };

            packageReferenceValidator.Execute().Should().BeTrue();
        }

        [Fact]
        public void CorrectlyValidatesPackageMinVersion()
        {
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var projectAssetsFile = Path.Combine(testDirectory, "test-project.assets.json");

            var packageReferenceValidator = new PackageReferenceValidator()
            {
                TargetFramework = "netcoreapp3.1",
                BuildEngine = new FakeBuildEngine(_output),
                ProjectAssetsFile = projectAssetsFile,
                ProjectFile = "Bob",
                RequiredPackages = new Microsoft.Build.Framework.ITaskItem[]
                {
                    new TaskItem("NerdBank.GitVersioning",
                        new Dictionary<string,string>
                        {
                            { "Version", "3.24.0" },
                            { "Filter", "Bob" }
                        }),
                    new TaskItem("NerdBank.GitVersioning",
                        new Dictionary<string,string>
                        {
                            { "MinVersion", "1.4" }
                        })
                }
            };

            packageReferenceValidator.Execute().Should().BeFalse();
        }

        [Fact]
        public void CorrectlyValidatesPackageMaxVersion()
        {
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var projectAssetsFile = Path.Combine(testDirectory, "test-project.assets.json");

            var packageReferenceValidator = new PackageReferenceValidator()
            {
                TargetFramework = "netcoreapp3.1",
                BuildEngine = new FakeBuildEngine(_output),
                ProjectAssetsFile = projectAssetsFile,
                ProjectFile = "Bob",
                RequiredPackages = new Microsoft.Build.Framework.ITaskItem[]
                {
                    new TaskItem("NerdBank.GitVersioning",
                        new Dictionary<string,string>
                        {
                            { "Version", "3.26.0" },
                            { "Filter", "Bob" }
                        }),
                    new  TaskItem("coverlet.collector",
                        new Dictionary<string,string>
                        {
                            { "MaxVersion", "1.2" },
                        }),
                }
            };

            packageReferenceValidator.Execute().Should().BeFalse();
        }

        [Fact]
        public void CorrectlyDeterminesForbiddenPackageIsPresent()
        {
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var projectAssetsFile = Path.Combine(testDirectory, "test-project.assets.json");

            var packageReferenceValidator = new PackageReferenceValidator()
            {
                TargetFramework = "netcoreapp3.1",
                BuildEngine = new FakeBuildEngine(_output),
                ProjectAssetsFile = projectAssetsFile,
                ProjectFile = "Bob",
                ForbiddenPackages = new Microsoft.Build.Framework.ITaskItem[]
                {
                    new TaskItem("NerdBank.GitVersioning")
                }
            };

            packageReferenceValidator.Execute().Should().BeFalse();
        }
    }
}
