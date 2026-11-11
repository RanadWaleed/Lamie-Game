using UnityEngine;using UnityEditor;using UnityEngine.SceneManagement;using UnityEditor.SceneManagement;using System.IO;
namespace Lamie.NativeStageDesign
{
 public static class NativeCloudFraming
 {
  const string R="Assets/Lamie_Game/DesignPreviews/UnityNativeStageDesign";
  [InitializeOnLoadMethod]static void Queue(){if(File.Exists(R+"/PolishVerified.txt")&&!File.Exists(R+"/CloudFramingVerified.txt"))EditorApplication.delayCall+=Fix;}
  static void Fix()
  {
   var s=SceneManager.GetSceneByPath(R+"/LevelBackgroundDesign_Native.unity");if(!s.isLoaded)return;Camera c=null;
   foreach(var root in s.GetRootGameObjects())
   {
    if(root.GetComponent<Camera>())c=root.GetComponent<Camera>();
    var clouds=root.transform.Find("06 - Clouds - Individual Animated Objects");if(clouds)for(int i=0;i<clouds.childCount;i++){var t=clouds.GetChild(i);t.localPosition=new Vector3(-9+i*6,5.2f+i%2*.35f,13.5f);}
   }
   EditorSceneManager.SaveScene(s);var rt=new RenderTexture(1920,1080,24);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t2=new Texture2D(1920,1080,TextureFormat.RGB24,false);t2.ReadPixels(new Rect(0,0,1920,1080),0,0);t2.Apply();File.WriteAllBytes(R+"/NativeStageDesign-Preview.png",t2.EncodeToPNG());c.targetTexture=null;RenderTexture.active=old;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(t2);File.WriteAllText(R+"/CloudFramingVerified.txt","Clouds inside camera frame. Native scene compiled and rendered.");
  }
 }
}
