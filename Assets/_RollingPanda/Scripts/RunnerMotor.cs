using UnityEngine;

namespace RollingPanda
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class RunnerMotor : MonoBehaviour
    {
        public const float LaneSpacing = 3f;
        public const float FinishDistance = 990f;
        const float ForwardSpeed = 10f;
        const float LateralSpeed = 12f;
        const float Gravity = -25f;
        const float JumpHeight = 2.2f;
        const float SlideDuration = 0.7f;
        CharacterController controller;
        Transform visual;
        Vector3 startPosition;
        float verticalSpeed, slideTime;
        int lane;
        bool paused;

        public bool Paused => paused;
        public bool Finished => transform.position.z >= FinishDistance;
        public bool Sliding => slideTime > 0f;

        public void Initialize(Transform playerVisual)
        {
            controller = GetComponent<CharacterController>();
            visual = playerVisual;
            startPosition = transform.position;
            SetStanding(true);
        }

        void Update()
        {
            if (controller == null) return;
            var commands = RunnerInput.Read();
            if (commands.Restart) { Restart(); return; }
            if (commands.TogglePause) paused = !paused;
            if (paused || Finished) return;

            float dt = Time.deltaTime;
            lane = Mathf.Clamp(lane + commands.LaneStep, -1, 1);
            bool grounded = controller.isGrounded;
            if (grounded && verticalSpeed < 0f) verticalSpeed = -2f;

            if (slideTime > 0f)
            {
                slideTime = Mathf.Max(0f, slideTime - dt);
                if (slideTime == 0f) SetStanding(true);
            }

            if (commands.Jump && grounded)
            {
                slideTime = 0f;
                SetStanding(true);
                verticalSpeed = Mathf.Sqrt(-2f * Gravity * JumpHeight);
            }
            else if (commands.Slide && grounded && !Sliding)
            {
                slideTime = SlideDuration;
                SetStanding(false);
            }

            verticalSpeed += Gravity * dt;
            float targetX = Mathf.MoveTowards(transform.position.x, lane * LaneSpacing, LateralSpeed * dt);
            float forward = Mathf.Min(ForwardSpeed * dt, FinishDistance - transform.position.z);
            var flags = controller.Move(new Vector3(targetX - transform.position.x, verticalSpeed * dt, forward));
            if ((flags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
        }

        void SetStanding(bool standing)
        {
            float height = standing ? 2f : 0.9f;
            controller.height = height;
            controller.center = Vector3.up * (height * 0.5f);
            if (visual == null) return;
            visual.localPosition = Vector3.up * (height * 0.5f);
            visual.localScale = new Vector3(0.85f, height * 0.5f, 0.85f);
        }

        public void Restart()
        {
            controller.enabled = false;
            transform.position = startPosition;
            controller.enabled = true;
            lane = 0;
            verticalSpeed = 0f;
            slideTime = 0f;
            paused = false;
            SetStanding(true);
        }
    }
}
