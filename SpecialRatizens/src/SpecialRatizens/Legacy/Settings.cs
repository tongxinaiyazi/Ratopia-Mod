using System;

namespace RatopiaMod
{
    /// <summary>
    /// 自定义游戏设置
    /// </summary>
    [Serializable]
    public class CustomSettings
    {
        //#region 开局设置

        ///// <summary>
        ///// 共享仓库
        ///// </summary>
        //public bool ShareStorage = false;

        //#endregion

        #region 游戏内设置

        /// <summary>
        /// 游戏速度加快
        /// </summary>
        public bool AddTimeScale = false;
        /// <summary>
        /// 时间流逝速度减半
        /// </summary>
        public bool TimeUpdateReducedSpeed = false;
        /// <summary>
        /// 人口限制翻倍
        /// </summary>
        public bool AddPopLimit = false;
        /// <summary>
        /// 无人机数量翻倍
        /// </summary>
        public bool AddDroneCount = false;
        /// <summary>
        /// 机器人数量翻倍
        /// </summary>
        public bool AddBotCount = false;
        /// <summary>
        /// 移民全正面特征
        /// </summary>
        public bool OnlyGoodCharacteristic = false;
        /// <summary>
        /// 猎人受伤减半
        /// </summary>
        public bool HunterBeAttackGetHalfDamage = false;
        /// <summary>
        /// 经验提高50%
        /// </summary>
        public bool MoreExp = false;
        /// <summary>
        /// 力量影响负重翻倍
        /// </summary>
        public bool MoreCapacityByPower = false;
        /// <summary>
        /// 默认选中已拾起的物品
        /// </summary>
        public bool DefaultSelectSameItem = false;
        /// <summary>
        /// 按住左键连续拾取物品
        /// </summary>
        public bool ContinuousTakeOutItems = false;
        /// <summary>
        /// 行动不丢弃拾取的物品
        /// </summary>
        public bool ActionNoDropGatheredItems = false;
        /// <summary>
        /// 自定义特殊单位
        /// </summary>
        public bool CustomSpecialUnit = false;
        /// <summary>
        /// 蓝图无需材料
        /// </summary>
        //public bool BluePrintNoNeedRes = false;
        /// <summary>
        /// 订单详细信息
        /// </summary>
        public bool SheetMoreInfo = false;
        /// <summary>
        /// 友方掉落无伤
        /// </summary>
        public bool DropNoDamgeWithOurTeam = false;
        /// <summary>
        /// 共享床位
        /// </summary>
        public bool ShareHome = false;
        /// <summary>
        /// 仓库容量翻倍
        /// </summary>
        public bool AddStorageCapacity = false;
        /// <summary>
        /// 自定义姓名
        /// </summary>
        public bool CustomNames = false;

        /// <summary>
        /// 移民性别限制
        /// </summary>
        public int NewCitizenGenderLimit = -1;
        /// <summary>
        /// 贸易通知
        /// </summary>
        public bool TradeResultMessage = false;
        /// <summary>
        /// 乌托邦模式
        /// </summary>
        public bool UtopiaMode = false;
        /// <summary>
        /// 和平模式
        /// </summary>
        public bool SafeMode = false;
        /// <summary>
        /// 敌方死亡掉落
        /// </summary>
        public bool EnemyDeadthDrop = false;
        /// <summary>
        /// 无限人口
        /// </summary>
        public bool PoPUnLimit = false;
        /// <summary>
        /// AI优化
        /// </summary>
        public bool OptimizeAI = false;
        /// <summary>
        /// 物品存取寻路优化
        /// </summary>
        public bool OptimizeBuyAndSellPathFind = false;
        /// <summary>
        /// 食物日用品寻路优化
        /// </summary>
        public bool OptimizeFoodAndLifePathFind = false;
        /// <summary>
        /// 娱乐卫生寻路优化
        /// </summary>
        public bool OptimizeGuestPathFind = false;
        /// <summary>
        /// 更多种植
        /// </summary>
        public bool MorePlantingPlants = false;
        /// <summary>
        /// 餐桌优化
        /// </summary>
        //public bool OptimizeFoodTable = false;

        /// <summary>
        /// 直供仓库
        /// </summary>
        public bool DirectSupplyStorage = false;

        /// <summary>
        /// 显示单位移动路径
        /// </summary>
        public bool DrawWay = false;
        /// <summary>
        /// 移除输入框字符长度限制
        /// </summary>
        public bool NameLengthUnLimit = false;

        #endregion
    }

    /// <summary>
    /// 游戏存档自定义设置
    /// </summary>
    [Serializable]
    public class GameSaveCustomSettings
    {
        /// <summary>
        /// 共享仓库
        /// </summary>
        public bool shareStorage = false;

        /// <summary>
        /// 自定义市民皮肤
        /// </summary>
        public bool customCitizenSkin = false;
    }
}
