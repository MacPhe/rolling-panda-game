using System.Collections;
using UnityEngine;

namespace RollingPanda
{
    // Carries the final chase view across the scene swap and dissolves it over the run.
    public sealed class RunEntryTransition : MonoBehaviour
    {
        public static bool Active { get; private set; }
        public static bool Revealing { get; private set; }
        public static float RevealProgress { get; private set; }
        Texture2D lastMenuFrame;
        public void Capture() { lastMenuFrame=ScreenCapture.CaptureScreenshotAsTexture(); }
        void Awake() { Active=true; DontDestroyOnLoad(gameObject); }
        public void Finish(AsyncOperation operation) { StartCoroutine(Reveal(operation)); }
        IEnumerator Reveal(AsyncOperation operation)
        {
            operation.allowSceneActivation=true;
            while(!operation.isDone) yield return null;
            // Let gameplay bootstrap and visual Start methods finish behind the cover.
            yield return null;
            yield return null;
            Revealing=true; RevealProgress=0;
            float elapsed=0;
            while(elapsed<.75f)
            {
                elapsed+=Time.unscaledDeltaTime;
                RevealProgress=Mathf.Clamp01(elapsed/.75f);
                yield return null;
            }
            RunnerInput.Reset();Revealing=false;Active=false;Destroy(gameObject);
        }
        void OnGUI()
        {
            if(lastMenuFrame==null) return;
            int depth=GUI.depth; GUI.depth=-10000;
            var matrix=GUI.matrix;var color=GUI.color;
            GUI.matrix=Matrix4x4.identity;
            GUI.color=new Color(1,1,1,1-Mathf.SmoothStep(0,1,RevealProgress));
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),lastMenuFrame);
            GUI.color=color;GUI.matrix=matrix;GUI.depth=depth;
        }
        void OnDestroy() { Active=false; Revealing=false;RevealProgress=0; if(lastMenuFrame!=null) Destroy(lastMenuFrame); }
    }
}
