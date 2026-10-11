#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace RollingPanda
{
    static class PandaLookStudyBuilder
    {
        const string Path="Assets/_RollingPanda/Scenes/PandaLookStudy.unity";
        [MenuItem("Rolling Panda/Open panda look study")]
        static void Open()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.Log("Exit Play Mode before opening the panda study.");return;}
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(System.IO.File.Exists(Path)){EditorSceneManager.OpenScene(Path);return;}
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("Original panda head and shoulders").AddComponent<PandaLookStudy>();
            var camera=new GameObject("Study camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.transform.position=new Vector3(.2f,1.5f,-4);camera.transform.LookAt(new Vector3(0,1.35f,0));
            camera.orthographic=true;camera.orthographicSize=1.25f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.34f,.32f,.29f);
            camera.gameObject.AddComponent<AudioListener>();
            var light=new GameObject("Soft studio key").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.color=new Color(1,.95f,.87f);light.transform.rotation=Quaternion.Euler(35,-140,0);light.shadows=LightShadows.None;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.48f,.48f,.5f);RenderSettings.fog=false;
            EditorSceneManager.SaveScene(scene,Path);
            Selection.activeGameObject=GameObject.Find("Original panda head and shoulders");
            Debug.Log("Panda look study created. Press Play to compare face, short fur, coat colours and removable headband. Gameplay scenes are unchanged.");
        }
    }
}
#endif
