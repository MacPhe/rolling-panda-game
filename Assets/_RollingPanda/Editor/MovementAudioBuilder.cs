#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace RollingPanda
{
    [InitializeOnLoad]
    static class MovementAudioBuilder
    {
        const string Path="Assets/_RollingPanda/Resources/Presentation/MovementAudioArt.asset";
        const string Source="Assets/Classic Footstep SFX/";
        static MovementAudioBuilder(){EditorApplication.delayCall+=Ensure;}
        static void Ensure(){Build(false);}
        [MenuItem("Rolling Panda/Refresh movement audio")]
        static void Refresh(){Build(true);}
        static void Build(bool refresh)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            var first=AssetDatabase.LoadAssetAtPath<AudioClip>(Source+"Rocky_ground/Rocky_ground_step0.wav");
            if(first==null) return;
            var art=AssetDatabase.LoadAssetAtPath<MovementAudioArt>(Path);
            if(!refresh && art!=null && art.Steps!=null && art.Steps.Length==8 && art.Jump!=null && art.Slide!=null) return;
            if(art==null) {System.IO.Directory.CreateDirectory("Assets/_RollingPanda/Resources/Presentation");AssetDatabase.Refresh();art=ScriptableObject.CreateInstance<MovementAudioArt>();AssetDatabase.CreateAsset(art,Path);}
            art.Steps=new AudioClip[8];
            for(int i=0;i<8;i++) art.Steps[i]=AssetDatabase.LoadAssetAtPath<AudioClip>(Source+"Rocky_ground/Rocky_ground_step"+i+".wav");
            art.Jump=AssetDatabase.LoadAssetAtPath<AudioClip>(Source+"Rocky_ground/Rocky_ground_jump0.wav");
            art.Slide=AssetDatabase.LoadAssetAtPath<AudioClip>(Source+"Sliding_on_ground0.wav");
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        }
    }
}
#endif
