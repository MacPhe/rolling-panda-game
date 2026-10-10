using System;
using RollingPanda;

static class Program
{
    static int count;
    static void Expect(SwipeDirection actual, SwipeDirection expected, string name)
    {
        if (actual != expected) throw new Exception(name + ": " + actual);
        count++;
    }
    static void Main()
    {
        var gesture = new SwipeGesture();
        foreach (var sample in new[] {
            (100f, 0f, SwipeDirection.Right), (-100f, 0f, SwipeDirection.Left),
            (0f, 100f, SwipeDirection.Up), (0f, -100f, SwipeDirection.Down) })
        {
            gesture.Begin(0, 0, 24, 0, false);
            Expect(gesture.Move(sample.Item1, sample.Item2, 0.1f), sample.Item3, "direction");
            Expect(gesture.Move(sample.Item1 * 2, sample.Item2 * 2, 0.2f), SwipeDirection.None, "one action per drag");
        }
        gesture.Begin(0, 0, 24, 0, false);
        Expect(gesture.Move(10, 5, 0.1f), SwipeDirection.None, "tap jitter");
        gesture.Begin(0, 0, 24, 0, false);
        Expect(gesture.Move(50, 50, 0.1f), SwipeDirection.None, "ambiguous diagonal");
        Expect(gesture.Move(100, 50, 0.2f), SwipeDirection.Right, "direction resolves");
        gesture.Begin(0, 0, 24, 0, true);
        Expect(gesture.Move(100, 0, 0.1f), SwipeDirection.None, "UI starts excluded");
        gesture.Begin(0, 0, 24, 0, false);
        gesture.Cancel();
        Expect(gesture.Move(100, 0, 0.1f), SwipeDirection.None, "cancelled touch");
        gesture.Begin(0, 0, 24, 0, false);
        Expect(gesture.Move(100, 0, 1f), SwipeDirection.None, "slow drag timeout");
        gesture.Begin(0, 0, 24, 2f, false);
        Expect(gesture.Move(0, 100, 2.1f), SwipeDirection.Up, "new gesture after reset");
        Console.WriteLine(count + " gesture regression checks passed.");
    }
}
