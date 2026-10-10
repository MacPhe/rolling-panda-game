namespace RollingPanda
{
    public static class ShieldRules
    {
        public const float Duration=10;
        public static bool Protects(float remaining) => remaining>0;
    }
}
