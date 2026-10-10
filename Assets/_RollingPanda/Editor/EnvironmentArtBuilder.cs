#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace RollingPanda
{
    [InitializeOnLoad]
    static class EnvironmentArtBuilder
    {
        const string Folder="Assets/_RollingPanda/Resources/Presentation";
        static EnvironmentArtBuilder(){EditorApplication.delayCall+=Build;EditorApplication.playModeStateChanged+=Changed;}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall+=Build;}
        [MenuItem("Rolling Panda/Refresh imported forest art")]
        static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(Load("Tree1_summer")==null) return;
            var art=AssetDatabase.LoadAssetAtPath<EnvironmentArt>(Folder+"/EnvironmentArt.asset");
            if(art!=null && art.SummerTrees!=null && art.SummerTrees.Length==8 && art.Petals!=null && art.MenuTree!=null && art.Magnet!=null) return;
            if(!AssetDatabase.IsValidFolder(Folder)) System.IO.Directory.CreateDirectory(Folder);
            if(art==null){art=ScriptableObject.CreateInstance<EnvironmentArt>();AssetDatabase.CreateAsset(art,Folder+"/EnvironmentArt.asset");}
            art.Magnet=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BTM_Assets/BTM_Items_Gems/Prefabs/Magnet.prefab");
            art.MenuTree=Load("Tree5_autumn");
            art.SummerTrees=Series("Tree",8,"summer");art.AutumnTrees=Series("Tree",8,"autumn");
            art.SummerBushes=Series("Bush",2,"summer");art.AutumnBushes=Series("Bush",2,"autumn");
            art.Logs=Series("Log",3,"summer");art.Stumps=Series("Stump",6,"summer");
            art.Petals=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/COMICOMI/VFX_FallingCherryBlossom/Art/Prefabs/FX_huaBan.prefab");
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("Rolling Panda: summer/autumn forest and shrine petals prepared.");
        }
        static GameObject Load(string name)=>AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LowPolyTrees/Prefabs/"+name+".prefab");
        static GameObject[] Series(string stem,int count,string season){var list=new GameObject[count];for(int i=0;i<count;i++) list[i]=Load(stem+(i+1)+"_"+season);return list;}
    }
}
#endif
