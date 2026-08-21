using System.Collections.Generic;

namespace EquipmentReforgeDodge.Core
{
    /// <summary>
    /// 纯逻辑规则：向重铸候选列表追加闪避属性，保持能力列表与数值列表严格同步。
    /// </summary>
    public static class DodgeCandidateRules
    {
        public static bool Contains(IEnumerable<Res_Ability> abilities, Res_Ability ability)
        {
            if (abilities == null)
            {
                return false;
            }

            foreach (var candidate in abilities)
            {
                if (candidate == ability)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsAppendable(IList<Res_Ability> abilities, IList<float> values)
        {
            return abilities != null && values != null && abilities.Count == values.Count;
        }

        /// <summary>
        /// 追加闪避候选。返回 false 表示数据不一致或已存在，不做任何修改。
        /// </summary>
        public static bool TryAppendDodge(IList<Res_Ability> abilities, IList<float> values, float dodgeValue)
        {
            if (!IsAppendable(abilities, values) || Contains(abilities, Res_Ability.Dodge))
            {
                return false;
            }

            abilities.Add(Res_Ability.Dodge);
            values.Add(dodgeValue);
            return true;
        }
    }
}
