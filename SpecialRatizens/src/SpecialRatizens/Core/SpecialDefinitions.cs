namespace SpecialRatizens.Core
{
    internal sealed class SpecialTraitDefinition
    {
        public SpecialTraitDefinition(
            int category,
            string name,
            string displayName,
            float effectValueA,
            float effectValueB,
            string description)
        {
            Category = category;
            Name = name;
            DisplayName = displayName;
            EffectValueA = effectValueA;
            EffectValueB = effectValueB;
            Description = description;
        }

        public int Category { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public float EffectValueA { get; }
        public float EffectValueB { get; }
        public string Description { get; }
    }

    internal sealed class SpecialRatizenDefinition
    {
        public SpecialRatizenDefinition(
            string name,
            string nameColor,
            string lockStatus,
            string gender,
            int grade,
            int power,
            int dexterity,
            int intelligence,
            int gold,
            string trait1,
            string icon1,
            string trait2,
            string icon2,
            int probability,
            string skin,
            string face,
            string beard,
            string dress,
            string glasses,
            string hair,
            string hat,
            string makeup)
        {
            Name = name;
            NameColor = nameColor;
            LockStatus = lockStatus;
            Gender = gender;
            Grade = grade;
            Power = power;
            Dexterity = dexterity;
            Intelligence = intelligence;
            Gold = gold;
            Trait1 = trait1;
            Icon1 = icon1;
            Trait2 = trait2;
            Icon2 = icon2;
            Probability = probability;
            Skin = skin;
            Face = face;
            Beard = beard;
            Dress = dress;
            Glasses = glasses;
            Hair = hair;
            Hat = hat;
            Makeup = makeup;
        }

        public string Name { get; }
        public string NameColor { get; }
        public string LockStatus { get; }
        public string Gender { get; }
        public int Grade { get; }
        public int Power { get; }
        public int Dexterity { get; }
        public int Intelligence { get; }
        public int Gold { get; }
        public string Trait1 { get; }
        public string Icon1 { get; }
        public string Trait2 { get; }
        public string Icon2 { get; }
        public int Probability { get; }
        public string Skin { get; }
        public string Face { get; }
        public string Beard { get; }
        public string Dress { get; }
        public string Glasses { get; }
        public string Hair { get; }
        public string Hat { get; }
        public string Makeup { get; }
        public bool IsUnlocked => LockStatus == "Unlock";
    }
}
