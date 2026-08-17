using System.IO;
using Xunit;

namespace SuperBow.Tests
{
    public sealed class ReleaseArtifactContractTests
    {
        [Fact]
        public void Chinese_readme_documents_identity_features_compatibility_and_safe_removal()
        {
            var path = Path.Combine(ContractTestPaths.ProjectRoot, "README.md");
            Assert.True(File.Exists(path), $"Missing README: {path}");
            var readme = File.ReadAllText(path);

            Assert.Contains("超级弓箭", readme);
            Assert.Contains("cn.ratopia.superbow", readme);
            Assert.Contains("0.1.2", readme);
            Assert.Contains("BepInEx\\plugins\\SuperBow\\SuperBow.dll", readme);
            Assert.Contains("范围攻击", readme);
            Assert.Contains("流血", readme);
            Assert.Contains("3%", readme);
            Assert.Contains("Boss", readme);
            Assert.Contains("AnimalBody", readme);
            Assert.Contains("MapObj", readme);
            Assert.Contains("EnemyNexus", readme);
            Assert.Contains("四舍五入", readme);
            Assert.Contains("C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D", readme);
            Assert.Contains("BloodDrain=3", readme);
            Assert.Contains("备份", readme);
            Assert.Contains("卸载", readme);
        }

        [Fact]
        public void Package_script_locks_environment_and_exact_archive_contents()
        {
            var path = Path.Combine(ContractTestPaths.ProjectRoot, "scripts", "Package.ps1");
            Assert.True(File.Exists(path), $"Missing package script: {path}");
            var script = File.ReadAllText(path);

            Assert.Contains("SuperBow-v0.1.2-BepInEx5.zip", script);
            Assert.Contains("BepInEx/plugins/SuperBow/SuperBow.dll", script);
            Assert.Contains("README.md", script);
            Assert.Contains("C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D", script);
            Assert.Contains("847D342FF36CD479790B39B6BA0D4159076C9995126E509FDE93961999A016C0", script);
            Assert.Contains("5.4.23.5", script);
            Assert.Contains("2.9.0.0", script);
            Assert.Contains("InstallAfterBuild=false", script);
            Assert.Contains("Assembly-CSharp.dll", script);
            Assert.Contains("UnityEngine*.dll", script);
            Assert.Contains(".pdb", script);
        }
    }
}
