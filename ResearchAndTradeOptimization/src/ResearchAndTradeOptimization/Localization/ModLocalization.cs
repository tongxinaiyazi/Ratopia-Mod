using System;
using System.Collections.Generic;
using I2.Loc;

namespace ResearchAndTradeOptimization.Localization
{
    internal static class ModLocalization
    {
        private static readonly IReadOnlyDictionary<string, string[]> Text =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["Modify"] = new[] { "调整", "調整", "Adjust", "조정" },
                ["ModifyDescription"] = new[]
                {
                    "调整这份贸易协议的每日数量和期限。商品、方向与仓库保持不变。",
                    "調整這份貿易協議的每日數量和期限。商品、方向與倉庫保持不變。",
                    "Adjust the daily quantity and term of this trade agreement. The goods, direction, and storage remain unchanged.",
                    "이 무역 협정의 일일 수량과 기간을 조정합니다. 상품, 방향, 창고는 변경되지 않습니다."
                },
                ["ModifyConfirm"] = new[]
                {
                    "确认应用新的数量和期限吗？本期将从现在重新开始，下一次交易起使用新设置。",
                    "確認套用新的數量和期限嗎？本期將從現在重新開始，下一次交易起使用新設定。",
                    "Apply the new quantity and term? The term restarts now and the new settings apply from the next trade.",
                    "새 수량과 기간을 적용하시겠습니까? 계약 기간은 지금부터 다시 시작하며 다음 거래부터 새 설정이 적용됩니다."
                },
                ["ModifySuccess"] = new[]
                {
                    "贸易协议已调整。",
                    "貿易協議已調整。",
                    "Trade agreement adjusted.",
                    "무역 협정이 조정되었습니다."
                },
                ["InvalidCount"] = new[]
                {
                    "当前繁荣度允许的最大贸易数量为 {0}。原有超限数量只能保持不变。",
                    "目前繁榮度允許的最大貿易數量為 {0}。原有超限數量只能保持不變。",
                    "The current prosperity allows at most {0}. An existing value above that limit may only be left unchanged.",
                    "현재 번영도에서 허용되는 최대 무역 수량은 {0}입니다. 기존 초과 수량은 그대로 유지할 때만 사용할 수 있습니다."
                },
                ["AgreementChanged"] = new[]
                {
                    "协议状态已经变化，请关闭面板后重试。",
                    "協議狀態已經變更，請關閉面板後重試。",
                    "The agreement state changed. Close the panel and try again.",
                    "협정 상태가 변경되었습니다. 창을 닫고 다시 시도하세요."
                }
            };

        internal static string Get(string key)
        {
            if (!Text.TryGetValue(key, out var values))
            {
                return key;
            }

            return values[GetLanguageIndex()];
        }

        internal static string Format(string key, params object[] arguments)
        {
            return string.Format(Get(key), arguments);
        }

        private static int GetLanguageIndex()
        {
            var code = LocalizationManager.CurrentLanguageCode ?? string.Empty;
            var normalized = code.Trim().ToLowerInvariant();
            if (normalized.StartsWith("ko", StringComparison.Ordinal))
            {
                return 3;
            }

            if (normalized.StartsWith("zh", StringComparison.Ordinal))
            {
                if (normalized.Contains("tw") ||
                    normalized.Contains("hk") ||
                    normalized.Contains("hant"))
                {
                    return 1;
                }

                return 0;
            }

            return 2;
        }
    }
}
