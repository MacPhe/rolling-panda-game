using UnityEngine;

namespace RollingPanda
{
    public sealed class RunnerCamera : MonoBehaviour
    {
        Transform target;
        Vector3 previousTargetPosition;
        readonly Vector3 offset = new Vector3(0f, 5f, -8f);

        public void Initialize(Transform player)
        {
            target = player;
            previousTargetPosition = player.position;
            Snap();
        }

        void LateUpdate()
        {
            if (target == null) return;
            // Snap after a restart so the camera never flies backward through the course.
            if (target.position.z < previousTargetPosition.z - 5f) Snap();
            else
            {
                Vector3 desired = new Vector3(target.position.x * 0.3f, 0f, target.position.z) + offset;
                transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-12f * Time.deltaTime));
                Aim();
            }
            previousTargetPosition = target.position;
        }

        void Snap()
        {
            transform.position = new Vector3(target.position.x * 0.3f, 0f, target.position.z) + offset;
            Aim();
        }

        void Aim() => transform.LookAt(new Vector3(0f, 1f, target.position.z + 6f));
    }
}
