namespace SuperBow.Runtime
{
    internal sealed class BowHitState
    {
        public BowHitState(
            T_Queen queen,
            RuntimeCombatTarget target,
            float healthBeforeVanilla,
            float directDamage,
            float centerX,
            float centerY)
        {
            Queen = queen;
            Target = target;
            HealthBeforeVanilla = healthBeforeVanilla;
            DirectDamage = directDamage;
            CenterX = centerX;
            CenterY = centerY;
        }

        public T_Queen Queen { get; }

        public RuntimeCombatTarget Target { get; }

        public float HealthBeforeVanilla { get; }

        public float DirectDamage { get; }

        public float CenterX { get; }

        public float CenterY { get; }
    }
}
