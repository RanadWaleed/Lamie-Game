using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.Rendering.Universal;
namespace Lamie.NativeStageDesign
{
    public static class NativeDesignRenderingFix
    {
        const string Root="Assets/Lamie_Game/DesignPreviews/UnityNativeStageDesign";
        [InitializeOnLoadMethod]
        static void Queue(){if(File.Exists(Root+"/LevelBackgroundDesign_Native.unity")&&!File.Exists(Root+"/RenderingVerified.txt"))EditorApplication.delayCall+=Fix;}
        static void Fix()
        {
            var scene=SceneManager.GetSceneByPath(Root+"/LevelBackgroundDesign_Native.unity");
            if(!scene.IsValid()||!scene.isLoaded)scene=EditorSceneManager.OpenScene(Root+"/LevelBackgroundDesign_Native.unity",OpenSceneMode.Additive);
            var shader=Shader.Find("Lamie/Native Illustration");if(!shader)throw new System.Exception("Native illustration shader not imported");
            foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{Root+"/Materials"}))
            {var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));var color=m.GetColor("_BaseColor");m.shader=shader;m.SetColor("_BaseColor",color);EditorUtility.SetDirty(m);}
            Camera camera=null;
            foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))
            {t.gameObject.layer=30;var c=t.GetComponent<Camera>();if(c)camera=c;}
            if(!camera)throw new System.Exception("Missing native camera");
            camera.cullingMask=1<<30;
            var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=false;data.renderShadows=false;
            // Render only the native design, even when another scene remains open additively.
            var rt=new RenderTexture(1920,1080,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
            var shot=new Texture2D(1920,1080,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,1920,1080),0,0);shot.Apply();File.WriteAllBytes(Root+"/NativeStageDesign-Preview.png",shot.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=old;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(shot);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllText(Root+"/RenderingVerified.txt","Isolated native geometry render with dedicated illustrated depth-tested shader. Existing scenes and render pipeline assets unchanged.");
            Debug.Log("LAMIE_NATIVE_RENDER_READY");
        }
    }
}
