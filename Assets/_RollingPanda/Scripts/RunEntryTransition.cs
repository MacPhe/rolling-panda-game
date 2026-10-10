using System.Collections;
using UnityEngine;

namespace RollingPanda
{
    // Survives the scene swap so the first game frame is covered, including slow loads.
    public sealed class RunEntryTransition : MonoBehaviour
    {
        public static bool Active { get; private set; }
        public float Opacity;
        void Awake() { Active=true; DontDestroyOnLoad(gameObject); }
        public void Finish(AsyncOperation operation) { StartCoroutine(Reveal(operation)); }
        IEnumerator Reveal(AsyncOperation operation)
        {
            operation.allowSceneActivation=true;
            while(!operation.isDone) yield return null;
            // Let gameplay bootstrap and visual Start methods finish behind the cover.
            yield return null;
            yield return null;
            float elapsed=0;
            while(elapsed<.5f)
            {
                elapsed+=Time.unscaledDeltaTime;
                Opacity=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.5f));
                yield return null;
            }
            RunnerInput.Reset();Active=false;Destroy(gameObject);
        }
        void OnGUI()
        {
            GUI.depth=-10000;
            var matrix=GUI.matrix;var color=GUI.color;
            GUI.matrix=Matrix4x4.identity;
            GUI.color=new Color(0,0,0,Opacity);
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
            GUI.color=color;GUI.matrix=matrix;
        }
        void OnDestroy() { Active=false; }
    }
}
