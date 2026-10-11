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
        RunnerMovementAudio movementAudio;
        Transform visual;
        Vector3 startPosition;
        float verticalSpeed, slideTime;
        int lane;
        bool paused;
        BambooChute chute;
        public bool InChute=>chute!=null;
        readonly Collider[] overlapResults = new Collider[16];
        public GameSession Session { get; set; }
        public bool Endless { get; set; }
        public double DistanceTravelled { get; private set; }
        public float CurrentSpeed => Session==null ? (Endless ? EndlessRules.Speed(DistanceTravelled) : ForwardSpeed) : Session.AdjustSpeed(Endless ? EndlessRules.Speed(DistanceTravelled) : ForwardSpeed);

        public int TargetLane => lane;
        public bool Paused => paused;
        public bool Finished => !Endless && transform.position.z >= FinishDistance;
        public bool Sliding => slideTime > 0f;

        public void Initialize(Transform playerVisual)
        {
            controller = GetComponent<CharacterController>();
            movementAudio = gameObject.AddComponent<RunnerMovementAudio>();
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
            if(!InChute)lane = Mathf.Clamp(lane + commands.LaneStep, -1, 1);
            bool grounded = controller.isGrounded;
            if (grounded && verticalSpeed < 0f) verticalSpeed = -2f;

            if(InChute)slideTime=Mathf.Max(slideTime,.1f);
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

            if (!InChute && commands.Jump && grounded && CanStand())
            {
                slideTime = 0f;
                SetStanding(true);
                movementAudio.Jumped();
                verticalSpeed = Mathf.Sqrt(-2f * Gravity * (Session==null ? JumpHeight : Session.JumpHeight(JumpHeight)));
            }
            else if (commands.Slide && grounded && !Sliding)
            {
                slideTime = SlideDuration;
                SetStanding(false);
                movementAudio.Slid();
            }

            verticalSpeed += Gravity * dt;
            float targetX = Mathf.MoveTowards(transform.position.x, lane * LaneSpacing, LateralSpeed * dt);
            float forward = Endless ? CurrentSpeed * dt : Mathf.Min(ForwardSpeed * dt, FinishDistance - transform.position.z);
            if(!InChute && Session!=null && Session.Track!=null)
            {
                bool front;var candidate=Session.Track.SweptChute(transform.position,new Vector3(targetX,transform.position.y,transform.position.z+forward),out front);
                if(candidate!=null)
                {
                    if(!front || !Sliding){Session.Fail("Roll into the bamboo chute entrance!");return;}
                    chute=candidate;lane=chute.Lane;slideTime=Mathf.Max(slideTime,.1f);SetStanding(false);targetX=chute.Centre;
                }
            }
            if(InChute){lane=chute.Lane;targetX=chute.Centre;}
            float beforeZ = transform.position.z;
            var flags = controller.Move(new Vector3(targetX - transform.position.x, verticalSpeed * dt, forward));
            DistanceTravelled += Mathf.Max(0f, transform.position.z - beforeZ);
            if(InChute && ChuteRules.HasExited(transform.position.z,chute.Exit))chute=null;
            if ((flags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
            if (Session != null && !Session.Defeated)
            {
                // Catch any obstacle overlapping the current capsule after movement.
                Vector3 center = transform.position + controller.center;
                float halfSegment = controller.height * 0.5f - controller.radius;
                int count = Physics.OverlapCapsuleNonAlloc(center + Vector3.up * halfSegment,
                    center - Vector3.up * halfSegment, controller.radius, overlapResults, ~0, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                    if (overlapResults[i].TryGetComponent<RunnerObstacle>(out var obstacle)) Session.HitObstacle(obstacle.Instruction);
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (Session != null && hit.collider.TryGetComponent<RunnerObstacle>(out var obstacle))
                Session.HitObstacle(obstacle.Instruction);
        }

        bool CanStand()
        {
            if(InChute)return false;
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
            chute=null;
            if(movementAudio!=null)movementAudio.ResetVocals();
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
