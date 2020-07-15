using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Common;
using NuGet.ProjectModel;
using NuGet.Versioning;

namespace NuGetPackageChecker
{
    public class PackageReferenceValidator : Task, ITask
    {
        [Required]
        public string ProjectFile { get; set; }

        [Required]
        public string ProjectAssetsFile { get; set; }

        [Required]
        public string TargetFramework { get; set; }

        [Required]
        public ITaskItem[] RequiredPackages { get; set; } = new ITaskItem[0];

        [Required]
        public ITaskItem[] ForbiddenPackages { get; set; } = new ITaskItem[0];

        public override bool Execute()
        {
            if (RequiredPackages.Length == 0 && ForbiddenPackages.Length == 0)
            {
                return true;
            }

            var lockFile = LockFileUtilities.GetLockFile(
                ProjectAssetsFile,
                new NullLogger());

            if (lockFile == null)
            {
                Log.LogError("Unable to load project assets file {0}", ProjectAssetsFile);
                return false;
            }

            var targetGroup = lockFile.Targets.SingleOrDefault(t => t.Name == GetFrameworkName(TargetFramework));

            if (targetGroup == null)
            {
                Log.LogError("Unable to find target for TargetFramework '{0}'", TargetFramework);
                return false;
            }

            return AllRequiredPackagesPresent(targetGroup) && NoForbiddenPackagesPresent(targetGroup);
        }


        private bool NoForbiddenPackagesPresent(LockFileTarget targetGroup)
        {
            bool noForbiddenPackagesPresent = true;
            foreach (var forbiddenPackage in ForbiddenPackages)
            {
                if (!IsFilterMatch(targetGroup, forbiddenPackage))
                {
                    continue;
                }

                var library = targetGroup.GetTargetLibrary(forbiddenPackage.ItemSpec);
                if (library == null)
                {
                    Log.LogMessage(MessageImportance.Normal, "Unable to find forbidden package {0} in '{1}' for TargetFramework {2}", forbiddenPackage.ItemSpec, ProjectFile, TargetFramework);
                    continue;
                }

                var version = forbiddenPackage.GetMetadata("Version");
                if (string.IsNullOrEmpty(version))
                {
                    Log.LogError("Reference to package {0} is forbidden in Project '{1}'", forbiddenPackage.ItemSpec, ProjectFile);
                    noForbiddenPackagesPresent = false;
                }
                else if (!NuGetVersion.TryParse(version, out NuGetVersion nugetVersion))
                {
                    Log.LogError("Unable to validate version of forbidden package {0} in Project '{1}'", forbiddenPackage.ItemSpec, ProjectFile);
                    noForbiddenPackagesPresent = false;
                }
                else if (library.Version == nugetVersion)
                {
                    Log.LogError("Reference to package {0} version {1} is forbidden in Project '{2}'", forbiddenPackage.ItemSpec, nugetVersion, ProjectFile);
                    noForbiddenPackagesPresent = false;
                }
                else
                {
                    Log.LogMessage(MessageImportance.Normal, "Package {0} in Project '{1}' does not match explicit version '{2}' of forbidden package so does not fail validation.", forbiddenPackage.ItemSpec, ProjectFile, nugetVersion);
                }
            }

            return noForbiddenPackagesPresent;
        }

        private bool AllRequiredPackagesPresent(LockFileTarget targetGroup)
        {
            bool allRequiredPackagesPresent = true;
            foreach (var requiredPackage in RequiredPackages)
            {
                if (!IsFilterMatch(targetGroup, requiredPackage))
                {
                    continue;
                }

                ValidationResult validationResult = ValidationResult.Success;
                var library = targetGroup.GetTargetLibrary(requiredPackage.ItemSpec);
                if (library == null)
                {
                    Log.LogError("Unable to find required package {0} in '{1}' for TargetFramework {2}", requiredPackage.ItemSpec, ProjectFile, TargetFramework);
                    validationResult = ValidationResult.Failure;
                }
                else
                {
                    validationResult = ValidateVersionIsValid(requiredPackage, library);
                }

                Log.LogMessage(MessageImportance.Normal, "Validation of package {0} in Project '{1}' TargetFramework '{2}' = '{3}'", requiredPackage.ItemSpec, ProjectFile, TargetFramework, validationResult);

                if (validationResult == ValidationResult.Failure)
                {
                    allRequiredPackagesPresent = false;
                }
            }

            return allRequiredPackagesPresent;
        }

        public enum ValidationResult
        {
            Success,
            Failure
        }

        private bool IsFilterMatch(LockFileTarget targetGroup, ITaskItem package)
        {
            var filter = package.GetMetadata("Filter");
            if (!string.IsNullOrEmpty(filter) && !Regex.IsMatch(Path.GetFileNameWithoutExtension(ProjectFile), filter))
            {
                Log.LogMessage(MessageImportance.Normal, "Project '{0}' does not match filter '{1}'. Skipping validation of '{2}'",
                    ProjectFile, filter, package.ItemSpec);
                return false;
            }

            var requiredFor = package.GetMetadata("RequiredFor");
            if (!string.IsNullOrEmpty(requiredFor) && targetGroup.GetTargetLibrary(requiredFor) == null)
            {
                Log.LogMessage(MessageImportance.Normal, "Project '{0}' does not reference '{1}' package. Skipping validation of '{2}'",
                    ProjectFile, requiredFor, package.ItemSpec);

                return false;
            }

            var targetFramework = package.GetMetadata("TargetFramework");
            if (!string.IsNullOrEmpty(targetFramework) && !Regex.IsMatch(TargetFramework, targetFramework))
            {
                Log.LogMessage(MessageImportance.Normal, "TargetFramework filter {0} of '{1}' does not match TargetFramework '{2}'. Skipping validation.",
                    targetFramework, package.ItemSpec, TargetFramework);
                return false;
            }

            return true;
        }

        public ValidationResult ValidateVersionIsValid(ITaskItem requiredPackage, LockFileTargetLibrary library)
        {
            var version = requiredPackage.GetMetadata("Version");
            if (!string.IsNullOrEmpty(version))
            {
                if (!NuGetVersion.TryParse(version, out NuGetVersion nugetVersion)
                    || library.Version != nugetVersion)
                {
                    Log.LogError("Version '{0}' of required package '{1}' does match required version '{2}'", library.Version, requiredPackage.ItemSpec, version);
                    return ValidationResult.Failure;
                }
            }

            var minVersion = requiredPackage.GetMetadata("MinVersion");
            if (!string.IsNullOrEmpty(minVersion))
            {
                if (!NuGetVersion.TryParse(minVersion, out NuGetVersion minNugetVersion)
                    || library.Version < minNugetVersion)
                {
                    Log.LogError("Version '{0}' of package '{1}' is less than minimum allowed version '{2}'", library.Version, requiredPackage.ItemSpec, minVersion);
                    return ValidationResult.Failure;
                }
            }

            var maxVersion = requiredPackage.GetMetadata("MaxVersion");
            if (!string.IsNullOrEmpty(maxVersion))
            {
                if (!NuGetVersion.TryParse(maxVersion, out NuGetVersion maxNugetVersion)
                    || library.Version > maxNugetVersion)
                {
                    Log.LogError("Version '{0}' of package '{1}' is greater than maximum allowed version '{2}'", library.Version, requiredPackage.ItemSpec, maxVersion);
                    return ValidationResult.Failure;
                }
            }

            return ValidationResult.Success;
        }

        private readonly Regex Net5Regex = new Regex("net(?<version>\\d+(\\.?)\\d*)(-(?<platform>\\D+))?(?<platformversion>\\d+.\\d+)?");

        public string GetFrameworkName(string targetFramework)
        {
            if (targetFramework.StartsWith("netcoreapp"))
            {
                var versionSuffix = targetFramework.Substring("netcoreapp".Length);

                return $".NETCoreApp,Version=v{versionSuffix}";
            }

            if (targetFramework.StartsWith("netstandard"))
            {
                var versionSuffix = targetFramework.Substring("netstandard".Length);

                return $".NETStandard,Version=v{versionSuffix}";
            }

            if (targetFramework.StartsWith("net"))
            {
                var versionSuffix = targetFramework.Substring("net".Length);
                if (Char.GetNumericValue(versionSuffix[0]) <= 4)
                {
                    return $".NETFramework,Version=v{string.Join(".", versionSuffix.Select(c => c.ToString()))}";
                }

                var match = Net5Regex.Match(targetFramework);
                string versionString = match.Groups["version"].Value;
                if (!versionString.Contains("."))
                {
                    versionString += ".0";
                }

                string platform = null;
                if (match.Groups["platform"].Success)
                {
                    platform = match.Groups["platform"].Value;
                }
                else
                {
                    return "net" + versionString;
                }

                string platformversion = "7.0";
                if (match.Groups["platformversion"].Success)
                {
                    platformversion = match.Groups["platformversion"].Value;
                }

                return $"net{versionString}-{platform}{platformversion}";
            }

            Log.LogError("Unable to determine Framework Name from TargetFramework '{0}'", targetFramework);
            return "Unknown";
        }
    }
}
