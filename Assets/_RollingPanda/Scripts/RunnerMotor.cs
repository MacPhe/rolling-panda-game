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
        const float SlideDuration = 1f;
        const float StandingHeight = 2f;
        const float SlidingHeight = 0.9f;
        CharacterController controller;
        Transform visual;
        Vector3 startPosition;
        float verticalSpeed, slideTime;
        int lane;
        bool paused;
        readonly Collider[] overlapResults = new Collider[16];
        public GameSession Session { get; set; }
        public bool Endless { get; set; }
        public double DistanceTravelled { get; private set; }
        public float CurrentSpeed => Endless ? EndlessRules.Speed(DistanceTravelled) : ForwardSpeed;

        public bool Paused => paused;
        public bool Finished => !Endless && transform.position.z >= FinishDistance;
        public bool Sliding => slideTime > 0f;

        public void Initialize(Transform playerVisual)
        {
            controller = GetComponent<CharacterController>();
            visual = playerVisual;
            startPosition = transform.position;
            RunnerInput.Reset();
            SetStanding(true);
        }

        void Update()
        {
            if (controller == null || RunEntryTransition.Active) return;
            var commands = RunnerInput.Read();
            if (commands.Restart) { Restart(); return; }
            if (commands.TogglePause) paused = !paused;
            if (paused || Finished || (Session != null && Session.Defeated)) return;

            float dt = Time.deltaTime;
            lane = Mathf.Clamp(lane + commands.LaneStep, -1, 1);
            bool grounded = controller.isGrounded;
            if (grounded && verticalSpeed < 0f) verticalSpeed = -2f;

            if (slideTime > 0f)
            {
                slideTime = Mathf.Max(0f, slideTime - dt);
                if (commands.SlideHeld) slideTime = Mathf.Max(slideTime, 0.1f);
                if (slideTime == 0f)
                {
                    // Never expand the capsule into a beam just because the timer ended.
                    if (CanStand()) SetStanding(true);
                    else slideTime = 0.05f;
                }
            }

            if (commands.Jump && grounded && CanStand())
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
            float forward = Endless ? CurrentSpeed * dt : Mathf.Min(ForwardSpeed * dt, FinishDistance - transform.position.z);
            float beforeZ = transform.position.z;
            var flags = controller.Move(new Vector3(targetX - transform.position.x, verticalSpeed * dt, forward));
            DistanceTravelled += Mathf.Max(0f, transform.position.z - beforeZ);
            if ((flags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
            if (Session != null && !Session.Defeated)
            {
                // Catch any obstacle overlapping the current capsule after movement.
                Vector3 center = transform.position + controller.center;
                float halfSegment = controller.height * 0.5f - controller.radius;
                int count = Physics.OverlapCapsuleNonAlloc(center + Vector3.up * halfSegment,
                    center - Vector3.up * halfSegment, controller.radius, overlapResults, ~0, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                    if (overlapResults[i].TryGetComponent<RunnerObstacle>(out var obstacle)) Session.Fail(obstacle.Instruction);
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (Session != null && hit.collider.TryGetComponent<RunnerObstacle>(out var obstacle))
                Session.Fail(obstacle.Instruction);
        }

        bool CanStand()
        {
            Vector3 center = transform.position + Vector3.up * (StandingHeight * 0.5f);
            float halfSegment = StandingHeight * 0.5f - controller.radius;
            int count = Physics.OverlapCapsuleNonAlloc(center + Vector3.up * halfSegment,
                center - Vector3.up * halfSegment, controller.radius + controller.skinWidth,
                overlapResults, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (overlapResults[i].TryGetComponent<RunnerObstacle>(out _)) return false;
            return count < overlapResults.Length;
        }

        void SetStanding(bool standing)
        {
            float height = standing ? StandingHeight : SlidingHeight;
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
            DistanceTravelled = 0;
            RunnerInput.Reset();
            SetStanding(true);
            if (Session != null) Session.ResetRun();
        }
        public void TogglePause() { paused = !paused; RunnerInput.Reset(); }
        public void ShiftOrigin(float shift)
        {
            controller.enabled = false;
            transform.position -= Vector3.forward * shift;
            controller.enabled = true;
        }
        void OnApplicationFocus(bool focused) { if (!focused) { paused = true; RunnerInput.Reset(); } }
        void OnApplicationPause(bool suspended) { if (suspended) { paused = true; RunnerInput.Reset(); } }
    }
}
