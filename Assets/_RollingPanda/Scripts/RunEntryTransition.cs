using System.Collections;
using UnityEngine;

namespace RollingPanda
{
    // Covers scene activation with a short fade; no screen-capture module required.
    public sealed class RunEntryTransition : MonoBehaviour
    {
        public static bool Active { get; private set; }
        public static bool Revealing { get; private set; }
        public static float RevealProgress { get; private set; }
        public float Opacity { get; set; }
        void Awake() { Active=true; DontDestroyOnLoad(gameObject); }
        public void Finish(AsyncOperation operation) { StartCoroutine(Reveal(operation)); }
        IEnumerator Reveal(AsyncOperation operation)
        {
            Opacity=1;
            if(operation!=null)
            {
                operation.allowSceneActivation=true;
                while(!operation.isDone) yield return null;
            }
            // Let gameplay bootstrap and visual Start methods finish behind the cover.
            yield return null;
            yield return null;
            Revealing=true; RevealProgress=0;
            float elapsed=0;
            while(elapsed<.75f)
            {
                elapsed+=Time.unscaledDeltaTime;
                RevealProgress=Mathf.Clamp01(elapsed/.75f);
                Opacity=1-Mathf.SmoothStep(0,1,RevealProgress);
                yield return null;
            }
            RunnerInput.Reset();Revealing=false;Active=false;Destroy(gameObject);
        }
        void OnGUI()
        {
            if(Opacity<=0) return;
            int depth=GUI.depth; GUI.depth=-10000;
            var matrix=GUI.matrix;var color=GUI.color;
            GUI.matrix=Matrix4x4.identity;
            GUI.color=new Color(0,0,0,Opacity);
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
            GUI.color=color;GUI.matrix=matrix;GUI.depth=depth;
        }
        void OnDestroy() { Active=false; Revealing=false;RevealProgress=0; }
    }
}

