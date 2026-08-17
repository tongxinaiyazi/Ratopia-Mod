using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SpecialRatizens.Core
{
    internal sealed class SpecialDataCatalog
    {
        private static readonly string[] RequiredTraitHeaders =
        {
            "Category", "Name", "T_Name", "EffectValue_A", "EffectValue_B", "Description"
        };

        private static readonly string[] RequiredRatizenHeaders =
        {
            "name", "nameColor", "LockStatus", "UnitGender", "grade", "pow", "dex", "wit", "gold",
            "char1", "icon1", "char2", "icon2", "probability", "skin", "face", "bread", "dress",
            "glasses", "hair", "hat", "makeup"
        };

        private SpecialDataCatalog(
            IReadOnlyList<SpecialRatizenDefinition> ratizens,
            IReadOnlyList<SpecialTraitDefinition> traits)
        {
            Ratizens = ratizens;
            Traits = traits;
        }

        public IReadOnlyList<SpecialRatizenDefinition> Ratizens { get; }
        public IReadOnlyList<SpecialTraitDefinition> Traits { get; }

        public static SpecialDataCatalog Load(string ratizenCsvPath, string traitCsvPath, string iconDirectory)
        {
            RequireFile(ratizenCsvPath, "特殊鼠鼠 CSV");
            RequireFile(traitCsvPath, "特性 CSV");
            if (!Directory.Exists(iconDirectory))
            {
                throw new InvalidDataException($"图标目录不存在：{iconDirectory}");
            }

            try
            {
                var traitTable = CsvTable.Parse(File.ReadAllText(traitCsvPath, Encoding.UTF8));
                var ratizenTable = CsvTable.Parse(File.ReadAllText(ratizenCsvPath, Encoding.UTF8));
                RequireHeaders(traitTable, RequiredTraitHeaders, "特性 CSV");
                RequireHeaders(ratizenTable, RequiredRatizenHeaders, "特殊鼠鼠 CSV");

                var traits = ParseTraits(traitTable);
                var ratizens = ParseRatizens(ratizenTable, traits, iconDirectory);
                return new SpecialDataCatalog(
                    new ReadOnlyCollection<SpecialRatizenDefinition>(ratizens),
                    new ReadOnlyCollection<SpecialTraitDefinition>(traits));
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception error) when (error is FormatException || error is OverflowException)
            {
                throw new InvalidDataException($"特殊鼠鼠数据格式错误：{error.Message}", error);
            }
        }

        private static List<SpecialTraitDefinition> ParseTraits(CsvTable table)
        {
            var result = new List<SpecialTraitDefinition>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in table.Rows)
            {
                var name = Required(row, "Name");
                if (!names.Add(name))
                {
                    throw new InvalidDataException($"特性名称重复：{name}");
                }

                var category = ParseInt(row, "Category");
                if (category != 0 && category != 1)
                {
                    throw new InvalidDataException($"特性 {name} 的 Category 必须为 0 或 1。");
                }

                result.Add(new SpecialTraitDefinition(
                    category,
                    name,
                    Required(row, "T_Name"),
                    ParseFloat(row, "EffectValue_A"),
                    ParseFloat(row, "EffectValue_B"),
                    Required(row, "Description")));
            }

            if (result.Count == 0)
            {
                throw new InvalidDataException("特性 CSV 没有数据。");
            }

            return result;
        }

        private static List<SpecialRatizenDefinition> ParseRatizens(
            CsvTable table,
            IReadOnlyList<SpecialTraitDefinition> traits,
            string iconDirectory)
        {
            var result = new List<SpecialRatizenDefinition>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            var traitNames = new HashSet<string>(traits.Select(item => item.Name), StringComparer.Ordinal);
            var traitOwners = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var row in table.Rows)
            {
                var name = Required(row, "name");
                if (!names.Add(name))
                {
                    throw new InvalidDataException($"特殊鼠鼠名称重复：{name}");
                }

                var lockStatus = Required(row, "LockStatus").Trim();
                if (lockStatus != "Unlock" && lockStatus != "Lock")
                {
                    throw new InvalidDataException($"特殊鼠鼠 {name} 的 LockStatus 无效：{lockStatus}");
                }

                var gender = Required(row, "UnitGender").Trim();
                if (gender != "Male" && gender != "Female")
                {
                    throw new InvalidDataException($"特殊鼠鼠 {name} 的性别无效：{gender}");
                }

                var probability = ParseInt(row, "probability");
                if (probability < 0 || probability > 10000)
                {
                    throw new InvalidDataException($"特殊鼠鼠 {name} 的概率必须在 0 到 10000 之间。");
                }

                var trait1 = Required(row, "char1");
                var trait2 = Required(row, "char2");
                if (!traitNames.Contains(trait1) || !traitNames.Contains(trait2))
                {
                    throw new InvalidDataException($"特殊鼠鼠 {name} 引用了不存在的特性：{trait1}/{trait2}");
                }
                if (lockStatus == "Unlock")
                {
                    AddTraitOwner(traitOwners, trait1, name);
                    AddTraitOwner(traitOwners, trait2, name);
                }

                var icon1 = Required(row, "icon1");
                var icon2 = Required(row, "icon2");
                RequireIcon(iconDirectory, name, icon1);
                RequireIcon(iconDirectory, name, icon2);

                result.Add(new SpecialRatizenDefinition(
                    name,
                    Required(row, "nameColor"),
                    lockStatus,
                    gender,
                    ParseInt(row, "grade"),
                    ParseInt(row, "pow"),
                    ParseInt(row, "dex"),
                    ParseInt(row, "wit"),
                    ParseInt(row, "gold"),
                    trait1,
                    icon1,
                    trait2,
                    icon2,
                    probability,
                    row["skin"].Trim(),
                    row["face"].Trim(),
                    row["bread"].Trim(),
                    row["dress"].Trim(),
                    row["glasses"].Trim(),
                    row["hair"].Trim(),
                    row["hat"].Trim(),
                    row["makeup"].Trim()));
            }

            if (result.Count == 0)
            {
                throw new InvalidDataException("特殊鼠鼠 CSV 没有数据。");
            }

            return result;
        }

        private static void AddTraitOwner(IDictionary<string, string> owners, string traitName, string ratizenName)
        {
            if (owners.TryGetValue(traitName, out var existingOwner))
            {
                throw new InvalidDataException(
                    $"特性 {traitName} 被多个特殊鼠鼠重复引用：{existingOwner}/{ratizenName}");
            }

            owners.Add(traitName, ratizenName);
        }

        private static string Required(IReadOnlyDictionary<string, string> row, string name)
        {
            var value = row[name].Trim();
            if (value.Length == 0)
            {
                throw new InvalidDataException($"字段 {name} 不能为空。");
            }
            return value;
        }

        private static int ParseInt(IReadOnlyDictionary<string, string> row, string name)
        {
            if (!int.TryParse(row[name].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                throw new InvalidDataException($"字段 {name} 不是有效整数：{row[name]}");
            }
            return value;
        }

        private static float ParseFloat(IReadOnlyDictionary<string, string> row, string name)
        {
            if (!float.TryParse(row[name].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                throw new InvalidDataException($"字段 {name} 不是有效数值：{row[name]}");
            }
            return value;
        }

        private static void RequireIcon(string iconDirectory, string ratizenName, string iconName)
        {
            var path = Path.Combine(iconDirectory, iconName + ".png");
            if (!File.Exists(path))
            {
                throw new InvalidDataException($"特殊鼠鼠 {ratizenName} 缺少图标：{path}");
            }
        }

        private static void RequireFile(string path, string label)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new InvalidDataException($"{label} 不存在：{path}");
            }
        }

        private static void RequireHeaders(CsvTable table, IEnumerable<string> headers, string label)
        {
            var available = new HashSet<string>(table.Headers, StringComparer.Ordinal);
            foreach (var header in headers)
            {
                if (!available.Contains(header))
                {
                    throw new InvalidDataException($"{label} 缺少字段：{header}");
                }
            }
        }
    }
}
