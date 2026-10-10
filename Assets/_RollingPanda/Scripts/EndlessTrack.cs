using UnityEngine;

namespace RollingPanda
{
    // A fixed pool: no new sections, obstacles or gems are created during a run.
    public sealed class EndlessTrack : MonoBehaviour
    {
        sealed class Section
        {
            public Transform Root;
            public GameObject[] Obstacles = new GameObject[6];
            public BlueGem[] Gems = new BlueGem[15];
        }
        readonly Section[] sections = new Section[EndlessRules.SectionCount];
        RunnerMotor runner;
        GameSession session;
        Material ground, lines, red, gold, purple, blue;
        long nextIndex;
        double origin;

        public void Initialize(RunnerMotor player, GameSession game, Material groundMaterial, Material lineMaterial,
            Material redMaterial, Material goldMaterial, Material purpleMaterial, Material blueMaterial)
        {
            runner = player; session = game;
            ground = groundMaterial; lines = lineMaterial; red = redMaterial;
            gold = goldMaterial; purple = purpleMaterial; blue = blueMaterial;
            for (int i = 0; i < sections.Length; i++) sections[i] = CreateSection(i);
            ResetTrack();
        }

        Section CreateSection(int number)
        {
            var section = new Section { Root = new GameObject("Track section " + number).transform };
            section.Root.SetParent(transform, false);
            Cube(section.Root, "Ground", new Vector3(0, -0.15f, 48), new Vector3(10, 0.3f, 96.04f), ground, true);
            foreach (float x in new[] { -4.8f, -1.5f, 1.5f, 4.8f })
                Cube(section.Root, "Lane line", new Vector3(x, 0.01f, 48), new Vector3(0.08f, 0.02f, 96), lines, false);
            for (int z = 0; z < 96; z += 12)
                Cube(section.Root, "Distance stripe", new Vector3(0, 0.015f, z), new Vector3(9.5f, 0.02f, 0.12f), lines, false);
            for (int row = 0; row < 3; row++)
            {
                for (int slot = 0; slot < 2; slot++)
                {
                    var obstacle = Cube(section.Root, "Obstacle", Vector3.zero, Vector3.one, red, true);
                    obstacle.AddComponent<RunnerObstacle>();
                    section.Obstacles[row * 2 + slot] = obstacle;
                }
                for (int g = 0; g < 5; g++)
                {
                    var gemObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    gemObject.name = "Pooled blue gem";
                    gemObject.transform.SetParent(section.Root, false);
                    gemObject.transform.localScale = Vector3.one * 0.65f;
                    gemObject.GetComponent<Renderer>().sharedMaterial = blue;
                    gemObject.GetComponent<SphereCollider>().isTrigger = true;
                    var body = gemObject.AddComponent<Rigidbody>();
                    body.isKinematic = true; body.useGravity = false;
                    var gem = gemObject.AddComponent<BlueGem>();
                    section.Gems[row * 5 + g] = gem;
                    session.RegisterGem(gem);
                }
            }
            return section;
        }

        static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool solid)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) { var collider = obj.GetComponent<Collider>(); collider.enabled = false; Destroy(collider); }
            return obj;
        }

        void Configure(Section section, long index)
        {
            section.Root.localPosition = new Vector3(0, 0, (float)(EndlessRules.Start(index) - origin));
            for (int row = 0; row < 3; row++)
            {
                int pattern = EndlessRules.Pattern(index, row);
                float z = EndlessRules.RowOffset(index, row);
                int count = EndlessRules.ObstacleCount(index, row);
                for (int slot = 0; slot < 2; slot++)
                {
                int lane = EndlessRules.ObstacleLane(index, row, slot);
                int kind = (pattern + slot * (1 + (int)(index % 2))) % 3;
                float height = kind == 0 ? 4.5f : kind == 1 ? 0.85f : 1.5f;
                var obstacle = section.Obstacles[row * 2 + slot];
                obstacle.transform.localPosition = new Vector3(lane * RunnerMotor.LaneSpacing, kind == 2 ? 1.35f + height / 2 : height / 2, z);
                obstacle.transform.localScale = new Vector3(2.2f, height, 1.2f);
                obstacle.GetComponent<Renderer>().sharedMaterial = kind == 0 ? red : kind == 1 ? gold : purple;
                obstacle.GetComponent<RunnerObstacle>().Instruction = kind == 0 ? "Dodge red blocks" : kind == 1 ? "Jump gold hurdles" : "Slide under purple beams";
                obstacle.SetActive(slot < count);
                }
                int safeLane = EndlessRules.SafeLane(index, row);
                for (int g = 0; g < 5; g++)
                {
                    var gem = section.Gems[row * 5 + g];
                    gem.transform.localPosition = new Vector3(safeLane * RunnerMotor.LaneSpacing, 1, z - 12 + g * 4);
                    gem.ResetGem();
                    if (count == 0) gem.gameObject.SetActive(false);
                }
            }
        }

        public void ResetTrack()
        {
            origin = 0; nextIndex = sections.Length;
            for (int i = 0; i < sections.Length; i++) Configure(sections[i], i);
            Physics.SyncTransforms();
        }

        void LateUpdate()
        {
            if (runner == null || runner.Paused || session.Defeated) return;
            bool changed = false;
            if (runner.transform.position.z >= EndlessRules.OriginStep)
            {
                origin += EndlessRules.OriginStep;
                changed = true;
                runner.ShiftOrigin(EndlessRules.OriginStep);
                foreach (var section in sections) section.Root.position -= Vector3.forward * EndlessRules.OriginStep;
                var camera = Camera.main;
                if (camera != null && camera.TryGetComponent<RunnerCamera>(out var follow)) follow.ShiftOrigin(EndlessRules.OriginStep);
            }
            foreach (var section in sections)
                if (section.Root.position.z + EndlessRules.SectionLength < runner.transform.position.z - 48f)
                {
                    Configure(section, nextIndex++);
                    changed = true;
                }
            if (changed) Physics.SyncTransforms();
        }
    }
}
