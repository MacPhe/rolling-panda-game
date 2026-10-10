using UnityEngine;

namespace RollingPanda
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class RunnerObstacle : MonoBehaviour
    {
        public string Instruction { get; set; }

        void Awake()
        {
            // Travel is +Z. Keep the approaching (-Z) face and vertical clearance,
            // but trim the trailing (+Z) face to forgive late lane changes.
            // Normalized values follow the pooled obstacle's changing scale/type.
            var box = GetComponent<BoxCollider>();
            box.center = new Vector3(0f, 0f, -0.2f);
            box.size = new Vector3(0.9f, 1f, 0.6f);
        }
    }
}
