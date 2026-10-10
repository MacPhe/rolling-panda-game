using UnityEngine.InputSystem;

namespace RollingPanda
{
    // Keyboard only for this checkpoint. Touch can supply the same commands later.
    public readonly struct RunnerCommands
    {
        public readonly int LaneStep;
        public readonly bool Jump, Slide, Restart, TogglePause;

        public RunnerCommands(int laneStep, bool jump, bool slide, bool restart, bool togglePause)
        {
            LaneStep = laneStep;
            Jump = jump;
            Slide = slide;
            Restart = restart;
            TogglePause = togglePause;
        }
    }

    public static class RunnerInput
    {
        public static RunnerCommands Read()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return default;
            bool left = keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame;
            bool right = keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame;
            return new RunnerCommands((right ? 1 : 0) - (left ? 1 : 0),
                keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame,
                keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame,
                keyboard.rKey.wasPressedThisFrame,
                keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame);
        }
    }
}
