using UnityEngine;

namespace RatopiaMod
{
    /// <summary>
    /// 自定义特殊单位
    /// </summary>
    internal class CustomSpecialUnit
    {
        public string name = "";

        public string nameColor = "";

        public Gender gender = Gender.Male;
        public string UnitGender { get { return gender.ToString(); } set { gender = BaseCommand.StringToEnum<Gender>((value ?? "").Trim()); } }

        public int grade = 0;

        public int pow = 0;

        public int dex = 0;

        public int wit = 0;

        public int gold = 0;

        public string char1 = "";

        public string icon1 = "";

        public string char2 = "";

        public string icon2 = "";

        public int probability = 0;

        public string skin = "";

        public string face = "";

        public string bread = "";

        public string dress = "";

        public string glasses = "";

        public string hair = "";

        public string hat = "";

        public string makeup = "";

        public string cheeck = "";

        public Lock_Status lockStatus = Lock_Status.Lock;
        public string LockStatus { get { return lockStatus.ToString(); } set { lockStatus = BaseCommand.StringToEnum<Lock_Status>(value); } }
        public enum Lock_Status
        { 
            Unlock,
            Lock,
        }

        //public CustomSpecialUnit(string name, Gender gender, int pow, int dex, int wit, int gold, int char1, CharacterInfo char2)
        //{
        //    this.name = name;
        //    this.gender = gender;
        //    this.pow = pow;
        //    this.dex = dex;
        //    this.wit = wit;
        //    this.gold = gold;
        //}

        #region 动态参数

        public string Name { get { return $"<color={nameColor}>{name}</color>"; } }

        public CharacterInfo char_1;

        public CharacterInfo char_2;

        public int pdr_C = 0;

        /// <summary>
        /// 真实概率值，每10次刷新值+1
        /// </summary>
        public int RealProbability { get { return probability + pdr_C; } }

        public bool isUsed = false;

        #endregion
    }
}
