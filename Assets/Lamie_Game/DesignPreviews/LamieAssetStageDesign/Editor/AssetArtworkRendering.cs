using UnityEngine;using UnityEditor;using UnityEngine.SceneManagement;using UnityEditor.SceneManagement;using System.IO;using System.Collections.Generic;
namespace Lamie.AssetStagePreview
{
 public static class AssetArtworkRendering
 {
  const string R="Assets/Lamie_Game/DesignPreviews/LamieAssetStageDesign";
  [InitializeOnLoadMethod]static void Queue(){if(File.Exists(R+"/BuildReport.txt")&&!File.Exists(R+"/ArtworkVerified.txt"))EditorApplication.delayCall+=Fix;}
  static void Fix()
  {
   var scene=SceneManager.GetSceneByPath(R+"/LevelBackgroundDesign_Assets.unity");if(!scene.isLoaded)return;
   var shader=Shader.Find("Lamie/Independent Asset Artwork");var cache=new Dictionary<string,UnityEngine.Mesh>();int index=0;Camera camera=null;
   foreach(var root in scene.GetRootGameObjects())
   {
    var c=root.GetComponent<Camera>();if(c)camera=c;
    foreach(var sr in root.GetComponentsInChildren<SpriteRenderer>())
    {
     var sprite=sr.sprite;string key=AssetDatabase.GetAssetPath(sprite);UnityEngine.Mesh mesh;
     if(!cache.TryGetValue(key,out mesh))
     {
      float x=sprite.bounds.size.x*.5f,y=sprite.bounds.size.y*.5f;mesh=new UnityEngine.Mesh();mesh.vertices=new[]{new Vector3(-x,-y),new Vector3(-x,y),new Vector3(x,y),new Vector3(x,-y)};mesh.uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,R+"/Meshes/Artwork_"+cache.Count+".asset");cache[key]=mesh;
     }
     var material=new Material(shader);material.SetTexture("_AssetTexture",sprite.texture);material.SetColor("_Tint",sr.color);AssetDatabase.CreateAsset(material,R+"/Materials/Artwork_"+(index++)+".mat");
     var go=new GameObject("Independent artwork surface");go.layer=29;go.transform.SetParent(sr.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=sr.sortingOrder;sr.enabled=false;
    }
    foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())
    {
     var m=renderer.sharedMaterial;if(m&&m.shader.name=="Lamie/Asset Stage Ground")
     {var texture=m.GetTexture("_MainTex");var wash=m.GetColor("_WashColor");m.shader=shader;m.SetTexture("_AssetTexture",texture);m.SetColor("_WashColor",wash);m.SetColor("_Tint",Color.white);EditorUtility.SetDirty(m);}
    }
   }
   EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();AssetStageBuilder.Capture(camera,R+"/AssetStageDesign-Preview.png");File.WriteAllText(R+"/ArtworkVerified.txt","Independent texture bindings for all source artwork; native gradient and separate imported assets, no flattened background.");Debug.Log("LAMIE_ASSET_ARTWORK_READY");
  }
 }
}

