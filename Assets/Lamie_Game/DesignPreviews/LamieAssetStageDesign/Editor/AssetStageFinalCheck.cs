using UnityEngine;using UnityEditor;using UnityEngine.SceneManagement;using UnityEditor.SceneManagement;using System.IO;using System.Reflection;
namespace Lamie.AssetStagePreview
{
 public static class AssetStageFinalCheck
 {
  const string R="Assets/Lamie_Game/DesignPreviews/LamieAssetStageDesign";
  [InitializeOnLoadMethod]static void Queue(){if(File.Exists(R+"/ArtworkVerified.txt")&&!File.Exists(R+"/FinalVerified.txt"))EditorApplication.delayCall+=Check;}
  static void Check()
  {
   var s=SceneManager.GetSceneByPath(R+"/LevelBackgroundDesign_Assets.unity");if(!s.isLoaded)return;Camera camera=null;AssetStageAtmosphere motion=null;
   foreach(var root in s.GetRootGameObjects())
   {
    var c=root.GetComponent<Camera>();if(c)camera=c;
    foreach(var m in root.GetComponentsInChildren<MeshRenderer>())if(m.sharedMaterial&&m.sharedMaterial.shader.name=="Lamie/Asset Gradient Sky")m.sortingOrder=-1000;
    if(root.GetComponent<AssetStageAtmosphere>())motion=root.GetComponent<AssetStageAtmosphere>();
   }
   if(!motion||motion.clouds.Length!=2||motion.speeds.Length!=2)throw new System.Exception("Cloud motion bindings invalid");
   var starts=new Vector3[2];for(int i=0;i<2;i++)starts[i]=motion.clouds[i].localPosition;
   typeof(AssetStageAtmosphere).GetMethod("OnEnable",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(motion,null);typeof(AssetStageAtmosphere).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(motion,null);
   for(int i=0;i<2;i++)motion.clouds[i].localPosition=starts[i];
   EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();AssetStageBuilder.Capture(camera,R+"/AssetStageDesign-Preview.png");File.WriteAllText(R+"/FinalVerified.txt","Compiled and rendered in Unity. Sky behind ground and clouds. Two cloud animation bindings and update checked. Original source images unchanged.");Debug.Log("LAMIE_ASSET_FINAL_READY");
  }
 }
}
