using System;

namespace RollingPanda
{
    public enum SwipeDirection { None, Left, Right, Up, Down }
    // Pure gesture rules allow regression checks without a device or Unity runtime.
    public sealed class SwipeGesture
    {
        float startX, startY, threshold, startTime;
        bool active;
        public void Begin(float x, float y, float minimumDistance, float time, bool blocked)
        {
            startX = x; startY = y; threshold = minimumDistance; startTime = time;
            active = !blocked;
        }
        public SwipeDirection Move(float x, float y, float time)
        {
            if (!active) return SwipeDirection.None;
            if (time - startTime > 0.8f) { Cancel(); return SwipeDirection.None; }
            float dx = x - startX, dy = y - startY;
            float ax = Math.Abs(dx), ay = Math.Abs(dy);
            if (Math.Max(ax, ay) < threshold) return SwipeDirection.None;
            if (Math.Max(ax, ay) < Math.Min(ax, ay) * 1.2f) return SwipeDirection.None;
            Cancel();
            return ax > ay ? (dx > 0f ? SwipeDirection.Right : SwipeDirection.Left)
                : (dy > 0f ? SwipeDirection.Up : SwipeDirection.Down);
        }
        public void Cancel() => active = false;
    }
}
