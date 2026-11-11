using UnityEngine;using UnityEditor;using UnityEngine.SceneManagement;using UnityEditor.SceneManagement;using System.IO;
namespace Lamie.NativeStageDesign
{
 public static class NativeDesignPolish
 {
  const string Root="Assets/Lamie_Game/DesignPreviews/UnityNativeStageDesign";
  [InitializeOnLoadMethod]static void Queue(){if(File.Exists(Root+"/RenderingVerified.txt")&&!File.Exists(Root+"/PolishVerified.txt"))EditorApplication.delayCall+=Polish;}
  static void Polish()
  {
   var s=SceneManager.GetSceneByPath(Root+"/LevelBackgroundDesign_Native.unity");if(!s.IsValid()||!s.isLoaded)return;
   Camera camera=null;Transform world=null;foreach(var r in s.GetRootGameObjects()){if(r.name.StartsWith("LAMIE"))world=r.transform;var c=r.GetComponent<Camera>();if(c)camera=c;}
   if(!camera||!world)return;camera.orthographicSize=10.3f;
   var sky=new Material(Shader.Find("Lamie/Native Gradient Sky"));AssetDatabase.CreateAsset(sky,Root+"/Materials/GradientSky.mat");
   var q=GameObject.CreatePrimitive(PrimitiveType.Quad);q.name="08 - Native procedural sunset sky - no image";SceneManager.MoveGameObjectToScene(q,s);q.transform.SetParent(camera.transform,false);q.transform.localPosition=new Vector3(0,0,80);q.transform.localScale=new Vector3(50,30,1);q.layer=30;q.GetComponent<Renderer>().sharedMaterial=sky;Object.DestroyImmediate(q.GetComponent<Collider>());
   var gold=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/WarmGold.mat");var wood=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/CarvedWood.mat");
   // Decorative roof teeth and carved side panels are all editable native geometry.
   var houses=world.Find("04 - Hijazi Architecture and Rawasheen");
   foreach(Transform h in houses)
   {
    var roof=h.Find("Roof cornice");float y=roof.localPosition.y;
    for(int i=-3;i<=3;i++){Cube("Decorative roof merlon",h,new Vector3(i*.46f,y+.28f,-1.45f),new Vector3(.25f,.4f,.3f),gold);}
    for(int side=-1;side<=1;side+=2)for(int row=0;row<3;row++)Cube("Carved facade accent",h,new Vector3(side*1.28f,1.1f+row*.9f,-1.79f),new Vector3(.12f,.45f,.08f),wood);
   }
   EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();
   var rt=new RenderTexture(1920,1080,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1920,1080,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1920,1080),0,0);t.Apply();File.WriteAllBytes(Root+"/NativeStageDesign-Preview.png",t.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(t);
   File.WriteAllText(Root+"/PolishVerified.txt","Native geometry, native procedural gradient sky, isolated camera; compiled and rendered in Unity.");Debug.Log("LAMIE_NATIVE_POLISH_READY");
  }
  static void Cube(string n,Transform p,Vector3 v,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=v;g.transform.localScale=scale;g.layer=30;g.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(g.GetComponent<Collider>());}
 }
}
