using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RatopiaMod
{
    [Serializable]
    public class CustomEnemyDrop
    {
        public EnemyType name;
        public string Name
        {
            get { return name.ToString(); }
            set { name = BaseCommand.StringToEnum<EnemyType>(value); }
        }

        public string T_Name;

        public List<TileDrop> dropList = new List<TileDrop>();
        public string DropList
        {
            get
            {
                return BaseCommand.ObjectToCsvText(dropList);
            }
            set
            {
                dropList = BaseCommand.CsvTextToObject<List<TileDrop>>(value) ?? new List<TileDrop>();
            }
        }
    }

    [Serializable]
    public class TileDrop
    {
        [JsonIgnore]
        public TileType name;
        public string Name
        {
            get { return name.ToString(); }
            set { name = BaseCommand.StringToEnum<TileType>(value); }
        }

        public int proValue;

        public int count;
    }
}
