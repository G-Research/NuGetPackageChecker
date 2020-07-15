import os
import subprocess

os.chdir(os.path.dirname(__file__))
print(f"Current working directory is {os.getcwd()}")
subprocess.run([".\Tests\TestNuGetPackageCheckerPackage\TestNuGetPackageCheckerPackage.cmd"], check=True)
subprocess.run([".\Tests\TestNuGetPackageCheckerPackageNet31\TestNuGetPackageCheckerPackage.cmd"], check=True)
