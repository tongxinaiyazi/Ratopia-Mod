using ScaffoldMod.Core;
using System.Linq;
using Utility.Savable;

namespace ScaffoldMod.Runtime
{
    internal static class ScaffoldSaveStore
    {
        internal const string RecordsKey = "cn.ratopia.scaffold.instances.v1";

        internal static ScaffoldRecord[] Load(D_Data data)
        {
            if (data?.ModsData == null || !data.ModsData.HasKey(RecordsKey))
            {
                return new ScaffoldRecord[0];
            }

            return ScaffoldRecordCodec.Decode(data.ModsData.GetValue<string>(RecordsKey, null)).ToArray();
        }

        internal static void Save(D_Data data, System.Collections.Generic.IEnumerable<ScaffoldRecord> records)
        {
            if (data == null)
            {
                return;
            }

            var snapshot = records.ToArray();
            if (snapshot.Length == 0)
            {
                if (data.ModsData != null && data.ModsData.HasKey(RecordsKey))
                {
                    data.ModsData.Remove(RecordsKey);
                }
                return;
            }

            if (data.ModsData == null)
            {
                data.ModsData = SavableData.Create();
            }
            data.ModsData.AddData(RecordsKey, ScaffoldRecordCodec.Encode(snapshot));
        }
    }
}
