using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RollingPanda
{
    public readonly struct RunnerCommands
    {
        public readonly int LaneStep;
        public readonly bool Jump, Slide, Restart, TogglePause, SlideHeld;

        public RunnerCommands(int laneStep, bool jump, bool slide, bool restart, bool togglePause, bool slideHeld)
        {
            LaneStep = laneStep;
            Jump = jump;
            Slide = slide;
            Restart = restart;
            TogglePause = togglePause;
            SlideHeld = slideHeld;
        }
    }

    public static class RunnerInput
    {
        static readonly SwipeGesture gesture = new SwipeGesture();
        static int fingerId = -1;
        public static Func<Vector2, bool> BlocksPointer;
        public static Func<Vector2, int> TouchButtonAt;
        static int touchButton, pendingButton;
        static Vector2 touchStart;

        public static void Reset() { gesture.Cancel(); fingerId = -1; touchButton = 0; pendingButton = 0; }
        static void Begin(Vector2 position)
        {
            float threshold = Mathf.Max(20f, Mathf.Min(Screen.width, Screen.height) * 0.06f);
            gesture.Begin(position.x, position.y, threshold, Time.unscaledTime,
                BlocksPointer != null && BlocksPointer(position));
        }
        static SwipeDirection ReadSwipe()
        {
            var screen = Touchscreen.current;
            if (screen != null)
            {
                var touch = screen.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    Reset(); fingerId = touch.touchId.ReadValue();
                    touchStart = touch.position.ReadValue(); Begin(touchStart);
                    touchButton = TouchButtonAt != null ? TouchButtonAt(touchStart) : 0;
                }
                if (fingerId != -1)
                {
                    if (touch.touchId.ReadValue() != fingerId || touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                    { Reset(); return SwipeDirection.None; }
                    var position = touch.position.ReadValue();
                    if ((position - touchStart).magnitude > Mathf.Max(20f, Mathf.Min(Screen.width, Screen.height) * 0.06f)) touchButton = 0;
                    var direction = gesture.Move(position.x, position.y, Time.unscaledTime);
                    if (!touch.press.isPressed)
                    {
                        int clicked = touchButton != 0 && TouchButtonAt != null && TouchButtonAt(position) == touchButton ? touchButton : 0;
                        Reset(); pendingButton = clicked;
                    }
                    return direction;
                }
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame) Begin(mouse.position.ReadValue());
                if (mouse.leftButton.isPressed || mouse.leftButton.wasReleasedThisFrame)
                {
                    var position = mouse.position.ReadValue();
                    var direction = gesture.Move(position.x, position.y, Time.unscaledTime);
                    if (mouse.leftButton.wasReleasedThisFrame) gesture.Cancel();
                    return direction;
                }
            }
#endif
            return SwipeDirection.None;
        }
        public static RunnerCommands Read()
        {
            var swipe = ReadSwipe();
            int clicked = pendingButton; pendingButton = 0;
            var keyboard = Keyboard.current;
            if (keyboard == null) return new RunnerCommands(swipe == SwipeDirection.Left ? -1 : swipe == SwipeDirection.Right ? 1 : 0,
                swipe == SwipeDirection.Up, swipe == SwipeDirection.Down, clicked == 1, clicked == 2, false);
            bool left = keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame;
            bool right = keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame;
            left |= swipe == SwipeDirection.Left;
            right |= swipe == SwipeDirection.Right;
            return new RunnerCommands((right ? 1 : 0) - (left ? 1 : 0),
                swipe == SwipeDirection.Up || keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame,
                swipe == SwipeDirection.Down || keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame,
                clicked == 1 || keyboard.rKey.wasPressedThisFrame,
                clicked == 2 || keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame,
                keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed);
        }
    }
}
