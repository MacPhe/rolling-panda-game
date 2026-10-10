using System;
using RollingPanda;

static class EndlessChecks
{
    public static void Run()
    {
        var seen = new bool[9];
        for (long section = 0; section < 10000; section++)
        {
            if (EndlessRules.Start(section + 1) - EndlessRules.Start(section) != EndlessRules.SectionLength)
                throw new Exception("Track section gap");
            for (int row = 0; row < 3; row++)
            {
                int pattern = EndlessRules.Pattern(section, row);
                if (pattern < 0 || pattern >= 9) throw new Exception("Invalid obstacle pattern");
                seen[pattern] = true;
                int safeLane = EndlessRules.SafeLane(section, row);
                int count = EndlessRules.ObstacleCount(section, row);
                int firstLane = EndlessRules.ObstacleLane(section, row, 0);
                int secondLane = EndlessRules.ObstacleLane(section, row, 1);
                if (safeLane < -1 || safeLane > 1 || safeLane == firstLane || safeLane == secondLane || firstLane == secondLane)
                    throw new Exception("Unsafe multi-obstacle row");
                if (count < 0 || count > 2 || (row >= EndlessRules.RowCount(section) && count != 0))
                    throw new Exception("Invalid row activation");
                double rowDistance = EndlessRules.Start(section) + EndlessRules.RowOffset(section, row);
                if (section > 0 && row < EndlessRules.RowCount(section))
                {
                    if (rowDistance < 500 && count != 1) throw new Exception("Opening difficulty changed");
                    if (rowDistance >= 1000 && count != 2) throw new Exception("Advanced row too easy");
                }
                if (EndlessRules.Pattern(section, row) != pattern) throw new Exception("Unstable restart pattern");
            }
        }
        foreach (bool value in seen) if (!value) throw new Exception("Pattern diversity missing");
        float previous = 0;
        for (int distance = 0; distance <= 100000; distance++)
        {
            float speed = EndlessRules.Speed(distance);
            if (speed < 10 || speed > 22 || speed < previous) throw new Exception("Unsafe speed ramp");
            previous = speed;
        }
        if (EndlessRules.Speed(0) != 10 || EndlessRules.Speed(1800) != 22 || EndlessRules.Speed(double.MaxValue) != 22)
            throw new Exception("Speed endpoints changed");
        double previousRow = -100;
        for (long section = 1; section < 10000; section++)
            for (int row = 0; row < EndlessRules.RowCount(section); row++)
            {
                double position = EndlessRules.Start(section) + EndlessRules.RowOffset(section, row);
                if (position - previousRow < 32) throw new Exception("Obstacle rows too close");
                previousRow = position;
            }
        if (32.0 / EndlessRules.MaximumSpeed < 6.0 / 12.0 + 0.75)
            throw new Exception("Insufficient two-lane transition margin");

        // Mathematical coverage/rebase stress check; physical seams still need Unity testing.
        double origin = 0;
        long next = EndlessRules.SectionCount;
        var indices = new long[EndlessRules.SectionCount];
        for (int i = 0; i < indices.Length; i++) indices[i] = i;
        int rebases = 0;
        for (double travelled = 0; travelled < 100000; travelled += 5.5)
        {
            double local = travelled - origin;
            if (local >= EndlessRules.OriginStep) { origin += EndlessRules.OriginStep; local -= EndlessRules.OriginStep; rebases++; }
            for (int i = 0; i < indices.Length; i++)
                if (EndlessRules.Start(indices[i]) - origin + EndlessRules.SectionLength < local - 48)
                    indices[i] = next++;
            var sorted = (long[])indices.Clone(); Array.Sort(sorted);
            for (int i = 1; i < sorted.Length; i++) if (sorted[i] != sorted[i - 1] + 1) throw new Exception("Pool continuity lost");
            if (travelled < EndlessRules.Start(sorted[0]) || travelled >= EndlessRules.Start(sorted[sorted.Length - 1]) + EndlessRules.SectionLength)
                throw new Exception("Player outside pooled ground coverage");
            if (local >= EndlessRules.OriginStep || local < 0) throw new Exception("Origin rebase failed");
        }
        if (rebases < 100) throw new Exception("Origin stress coverage missing");
        Console.WriteLine("Endless rules passed: 30,000 row slots, multi-lane safety, spacing, 100,001 speed samples, 100 km pool/rebase simulation.");
    }
}
