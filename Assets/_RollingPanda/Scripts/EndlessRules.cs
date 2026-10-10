using System;

namespace RollingPanda
{
    public static class EndlessRules
    {
        public const int SectionCount = 8;
        public const float SectionLength = 96f;
        public const float OriginStep = 960f;
        public static float Speed(double distance) => (float)Math.Min(18.0, 10.0 + Math.Max(0.0, distance) / 250.0);
        public static double Start(long index) => index * (double)SectionLength - 48.0;
        // Stable patterns make restarts comparable without affecting Unity's global RNG.
        public static int Pattern(long section, int row)
        {
            unchecked
            {
                uint value = (uint)section * 747796405u + (uint)row * 2891336453u + 1729u;
                value = (value ^ (value >> 16)) * 2246822519u;
                return (int)((value ^ (value >> 13)) % 9u);
            }
        }
    }
}
