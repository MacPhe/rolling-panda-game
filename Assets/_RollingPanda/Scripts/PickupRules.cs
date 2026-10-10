namespace RollingPanda
{
    public enum PickupKind { Blue, Red, Pouch }
    public static class PickupRules
    {
        public static int Points(PickupKind kind) => kind == PickupKind.Red ? 25 : kind == PickupKind.Blue ? 10 : 0;
        public static PickupKind Gem(double distance, long section, int row, int gem) =>
            distance > 1000 && ((section + row) % 3 == 1 || gem == 2) ? PickupKind.Red : PickupKind.Blue;
        public static bool HasPouch(long section) => EndlessRules.Start(section) + 24 > 2250 && section % 6 == 0;
        public static int PouchSlot(long section) => EndlessRules.ObstacleKind(section, 0, 0) == 1 ? 0
            : EndlessRules.ObstacleKind(section, 0, 1) == 1 ? 1 : EndlessRules.ObstacleKind(section, 0, 0) == 2 ? 0 : 1;
        public static float PouchHeight(long section) => EndlessRules.ObstacleKind(section, 0, PouchSlot(section)) == 1 ? 3.7f : 0.28f;
    }
    public sealed class RunPickups
    {
        public int Blue { get; private set; }
        public int Red { get; private set; }
        public int Pouches { get; private set; }
        public long Points => (long)Blue * 10 + (long)Red * 25;
        public void Collect(PickupKind kind)
        {
            if (kind == PickupKind.Blue) Blue++;
            else if (kind == PickupKind.Red) Red++;
            else Pouches++;
        }
        public void Reset() { Blue = 0; Red = 0; Pouches = 0; }
    }
}
