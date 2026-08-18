using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class ReleaseArtifactContractTests
    {
        [Fact]
        public void Readme_documents_the_release_identity_installation_and_safety_contract()
        {
            var readme = ReadRequiredRepositoryFile("README.md");

            Assert.Contains("0.1.2", readme);
            Assert.Contains("C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D", readme);
            Assert.Contains("2026-07-24", readme);
            Assert.Contains("BepInEx 5", readme);
            Assert.Contains("BepInEx\\plugins\\EquipmentReforgeSelector\\EquipmentReforgeSelector.dll", readme);
            Assert.Contains("原版随机", readme);
            Assert.Contains("存档", readme);
            Assert.Contains("卸载", readme);
            Assert.Contains("LogOutput.log", readme);
            Assert.Contains("整行", readme);
            Assert.Contains("数字键", readme);
            Assert.Contains("不会清除", readme);
        }

        [Fact]
        public void Testing_document_covers_the_agreed_manual_acceptance_matrix()
        {
            var testing = ReadRequiredRepositoryFile(Path.Combine("docs", "TESTING.md"));

            Assert.Contains("Royal", testing);
            Assert.Contains("1 级", testing);
            Assert.Contains("HellAnvil", testing);
            Assert.Contains("2 级", testing);
            Assert.Contains("武器", testing);
            Assert.Contains("衣服", testing);
            Assert.Contains("饰品", testing);
            Assert.Contains("鼠标", testing);
            Assert.Contains("键盘", testing);
            Assert.Contains("材料", testing);
            Assert.Contains("数值", testing);
            Assert.Contains("跨等级", testing);
            Assert.Contains("两轮", testing);
            Assert.Contains("临时移除", testing);
            Assert.Contains("日志", testing);
            Assert.Contains("四个格子", testing);
            Assert.Contains("保持", testing);
        }

        [Fact]
        public void Package_script_has_explicit_build_test_switches_and_exact_safe_layout()
        {
            var script = ReadRequiredRepositoryFile(Path.Combine("scripts", "Package.ps1"));

            Assert.Contains("[switch]$Build", script);
            Assert.Contains("[switch]$Test", script);
            Assert.Contains("InstallAfterBuild=false", script);
            Assert.Contains("BepInEx", script);
            Assert.Contains("plugins", script);
            Assert.Contains("EquipmentReforgeSelector", script);
            Assert.Contains("Mono.Cecil", script);
            Assert.Contains("Assembly-CSharp", script);
            Assert.Contains("UnityEngine", script);
            Assert.Contains("BepInEx", script);
            Assert.Contains("0Harmony", script);
            Assert.Contains("pdb", script);
            Assert.Contains("bin", script);
            Assert.Contains("obj", script);
            Assert.Contains("log", script);
            Assert.Contains("save", script);
            Assert.Contains("v0.1.2-BepInEx5.zip", script);
        }

        [Fact]
        public void Package_script_creates_the_exact_release_zip_from_a_valid_release_dll()
        {
            var scriptPath = Path.Combine(ContractTestPaths.RepositoryRoot, "scripts", "Package.ps1");
            var archivePath = Path.Combine(ContractTestPaths.RepositoryRoot, "dist", "装备重铸自选属性-v0.1.2-BepInEx5.zip");
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                WorkingDirectory = ContractTestPaths.RepositoryRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                process.WaitForExit();

                Assert.True(process.ExitCode == 0, $"Package script failed: {output}");
            }

            Assert.True(File.Exists(archivePath), $"Expected package is missing: {archivePath}");

            var expectedEntries = new[]
            {
                "README.md",
                "docs/TESTING.md",
                "BepInEx/plugins/EquipmentReforgeSelector/EquipmentReforgeSelector.dll"
            };
            string[] actualEntries;

            using (var stream = File.OpenRead(archivePath))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                actualEntries = archive.Entries
                    .Where(entry => !string.IsNullOrEmpty(entry.Name))
                    .Select(entry => entry.FullName.Replace('\\', '/'))
                    .OrderBy(entry => entry, StringComparer.Ordinal)
                    .ToArray();
            }

            var missingEntries = expectedEntries.Except(actualEntries, StringComparer.Ordinal).ToArray();
            var unexpectedEntries = actualEntries.Except(expectedEntries, StringComparer.Ordinal).ToArray();
            Assert.Empty(missingEntries);
            Assert.Empty(unexpectedEntries);
            Assert.DoesNotContain(actualEntries, entry => entry.StartsWith("package-staging/", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(actualEntries, IsForbiddenPackageEntry);
            Assert.Equal(expectedEntries.OrderBy(entry => entry, StringComparer.Ordinal), actualEntries);
        }

        private static bool IsForbiddenPackageEntry(string entry)
        {
            var fileName = Path.GetFileName(entry);
            return entry.IndexOf("/bin/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                entry.IndexOf("/obj/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                entry.IndexOf("/save/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                entry.IndexOf("/saves/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                fileName.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".log", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".sav", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("Assembly-CSharp.dll", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("Assembly-CSharp-firstpass.dll", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("0Harmony.dll", StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith("BepInEx", StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith("UnityEngine", StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith("Harmony", StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith("Mono.Cecil", StringComparison.OrdinalIgnoreCase) ||
                (fileName.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
        }

        private static string ReadRequiredRepositoryFile(string relativePath)
        {
            var path = Path.Combine(ContractTestPaths.RepositoryRoot, relativePath);
            Assert.True(File.Exists(path), $"Required release file is missing: {path}");
            return File.ReadAllText(path);
        }
    }
}
