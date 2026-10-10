using System;

namespace RollingPanda
{
    public static class EndlessRules
    {
        public const int SectionCount = 8;
        public const float SectionLength = 96f;
        public const float OriginStep = 960f;
        public const float MaximumSpeed = 26f;
        public static float Speed(double distance) => (float)Math.Min(MaximumSpeed, 14.0 + Math.Max(0.0, distance) / 150.0);
        public static double Start(long index) => index * (double)SectionLength - 48.0;
        public static int RowCount(long section) => Start(section) >= 2000 ? 3 : 2;
        public static float RowOffset(long section, int row) => RowCount(section) == 3 ? 16 + row * 32 : 24 + row * 48;
        public static int ObstacleCount(long section, int row)
        {
            if (section == 0 || row >= RowCount(section)) return 0;
            double distance = Start(section) + RowOffset(section, row);
            if (distance < 150) return 1;
            if (distance < 300) return (section + row) % 2 == 0 ? 2 : 1;
            return 2;
        }
        public static int SafeLane(long section, int row) => (Pattern(section, row) / 3 + 1) % 3 - 1;
        public static int ObstacleLane(long section, int row, int slot)
        {
            int primary = Pattern(section, row) / 3 - 1;
            return slot == 0 ? primary : -primary - SafeLane(section, row);
        }
        public static int ObstacleKind(long section, int row, int slot) => (Pattern(section, row) + slot * (1 + (int)(section % 2))) % 3;
        public static int GemObstacleSlot(long section, int row)
        {
            if (Start(section) + RowOffset(section, row) <= 300 || (section + row) % 3 == 0) return -1;
            int count = ObstacleCount(section, row);
            for (int attempt = 0; attempt < count; attempt++)
            {
                int slot = ((int)((section + row) % count) + attempt) % count;
                if (ObstacleKind(section, row, slot) != 0) return slot;
            }
            return -1;
        }
        public static float GemHeight(int obstacleKind, int gem)
        {
            if (obstacleKind == 2) return 0.45f;
            if (obstacleKind == 1) return gem == 2 ? 2.8f : gem == 0 || gem == 4 ? 1.8f : 2.6f;
            return 1f;
        }
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
