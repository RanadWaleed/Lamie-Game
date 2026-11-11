using UnityEngine;using UnityEditor;using UnityEngine.SceneManagement;using UnityEditor.SceneManagement;using UnityEngine.Rendering.Universal;using System.IO;using System.Collections.Generic;
namespace Lamie.AssetStagePreview
{
 public static class AssetStageBuilder
 {
  const string R="Assets/Lamie_Game/DesignPreviews/LamieAssetStageDesign";
  static Transform root;static List<Transform> clouds;static Material spriteMat;
  [InitializeOnLoadMethod]static void Queue(){if(File.Exists(R+"/BuildRequested.txt")&&!File.Exists(R+"/LevelBackgroundDesign_Assets.unity"))EditorApplication.delayCall+=Build;}
  [MenuItem("Lamie/Design Previews/Build Asset Stage Design")]
  public static void Build()
  {
   if(File.Exists(R+"/LevelBackgroundDesign_Assets.unity"))return;
   Directory.CreateDirectory(R+"/Materials");Directory.CreateDirectory(R+"/Meshes");AssetDatabase.Refresh();
   foreach(string file in Directory.GetFiles(R+"/Art","*.png",SearchOption.AllDirectories))
   {string importPath=file.Replace('\\','/');var im=(TextureImporter)AssetImporter.GetAtPath(importPath);im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Single;im.spritePixelsPerUnit=100;im.alphaIsTransparency=true;im.mipmapEnabled=false;im.maxTextureSize=importPath.Contains("Clouds")?4096:2048;im.textureCompression=TextureImporterCompression.Uncompressed;im.wrapMode=importPath.Contains("land_")||importPath.Contains("road_")?TextureWrapMode.Repeat:TextureWrapMode.Clamp;im.SaveAndReimport();}
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
   root=new GameObject("LAMIE - Asset Based Stage Design").transform;root.gameObject.layer=29;clouds=new List<Transform>();
   spriteMat=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));spriteMat.name="Imported Art Unlit";AssetDatabase.CreateAsset(spriteMat,R+"/Materials/ImportedArt.mat");
   var camera=new GameObject("Asset Design Camera").AddComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.orthographicSize=9;camera.nearClipPlane=.1f;camera.farClipPlane=200;camera.cullingMask=1<<29;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.64f,.55f,.78f);camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
   // Native shader gradient, no sky image.
   var skyMat=new Material(Shader.Find("Lamie/Asset Gradient Sky"));AssetDatabase.CreateAsset(skyMat,R+"/Materials/Sky.mat");
   var sky=GameObject.CreatePrimitive(PrimitiveType.Quad);sky.name="01 - Native Gradient Sunset Sky";sky.layer=29;sky.transform.SetParent(root,false);sky.transform.localPosition=new Vector3(0,0,30);sky.transform.localScale=new Vector3(40,24,1);sky.GetComponent<Renderer>().sharedMaterial=skyMat;Object.DestroyImmediate(sky.GetComponent<Collider>());
   var distant=Group("02 - Distant Palm Silhouettes");
   for(int i=0;i<11;i++){float h=1.3f+(i%3)*.23f;var s=Sprite("Distant palm "+i,"Environment/tree_2.png",new Vector2(-15+i*3,1.3f),h,-15,distant);s.color=new Color(.72f,.58f,.68f,.32f);}
   var ground=Group("03 - Ready Made Textured Ground");
   var groundMat=Textured("Warm sand from CraftPix","Environment/land_5.png",new Color(1,.87f,.7f,.57f));
   var verts=new List<Vector3>();var uvs=new List<Vector2>();var tri=new List<int>();
   for(int i=0;i<=80;i++){float x=-18+i*.45f;float y=1.6f+Mathf.Sin(i*.12f)*.28f;verts.Add(new Vector3(x,-10,0));verts.Add(new Vector3(x,y,0));uvs.Add(new Vector2(x/2,0));uvs.Add(new Vector2(x/2,(y+10)/2));if(i<80){int k=i*2;tri.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});}}
   Mesh("Sand surface - tiled source texture",verts.ToArray(),uvs.ToArray(),tri.ToArray(),groundMat,-20,ground);
   var path=Group("04 - Curved Stone Path");
   var knots=new[]{new Vector2(11,-9),new Vector2(10,-6),new Vector2(8,-5),new Vector2(5,-4.7f),new Vector2(2,-3.7f),new Vector2(-1,-3.2f),new Vector2(-4,-2.5f),new Vector2(-7,-1.5f),new Vector2(-10,-1.1f)};
   var pts=Smooth(knots);
   Ribbon("Stone path outer edge",pts,1.95f,Textured("Path border","Environment/road_5.png",new Color(.64f,.43f,.31f,.55f)),-9,path);
   Ribbon("Ready made stone paving",pts,1.6f,Textured("Cream cobbled path","Environment/road_5.png",new Color(1,.91f,.72f,.42f)),-8,path);
   var stages=Group("05 - Your Three Stage Buildings");
   // Original user files are copied intact and individually imported.
   Sprite("Stage 3 - Library","Buildings/Stage03.png",new Vector2(-9,1.35f),6.2f,10,stages);
   Sprite("Stage 2 - Theatre","Buildings/Stage02.png",new Vector2(-.1f,-.15f),6.9f,20,stages);
   Sprite("Stage 1 - Shop","Buildings/Stage01.png",new Vector2(9,-1.8f),7.6f,30,stages);
   var foliage=Group("06 - Ready Made Palms and Plants");
   Sprite("Left framing palms","Environment/tree_1.png",new Vector2(-14.3f,-2),5.7f,35,foliage);
   Sprite("Right framing palm","Environment/tree_3.png",new Vector2(14,-3.2f),5.3f,35,foliage);
   Sprite("Back left palm","Environment/tree_2.png",new Vector2(-13.5f,1.5f),3.4f,4,foliage);
   Sprite("Back right palms","Environment/tree_9.png",new Vector2(12.8f,1.5f),3.6f,4,foliage);
   Sprite("Middle distant palm","Environment/tree_2.png",new Vector2(4.8f,1.2f),2.8f,5,foliage);
   for(int i=0;i<14;i++){float x=-15+i*2.3f;float y=-7.4f+Mathf.Sin(i*2.1f)*.5f;Sprite("Foreground plant "+i,"Environment/greenery_1.png",new Vector2(x,y),.6f+(i%3)*.15f,40,foliage);}
   foreach(var p in new[]{new Vector2(-12,-6),new Vector2(-5,-5.2f),new Vector2(4,-6.8f),new Vector2(13,-6)})Sprite("Small stone detail","Environment/stones_1.png",p,.55f,8,foliage);
   var cloudLayer=Group("07 - Handpainted GOBI Cloud Layers");
   var c1=Sprite("Cloud layer far","Clouds/clouds2_color.png",new Vector2(-1,6.5f),3.5f,-12,cloudLayer);c1.color=new Color(1,.88f,.97f,.55f);clouds.Add(c1.transform);
   var c2=Sprite("Cloud layer near","Clouds/clouds1_color.png",new Vector2(2,5.7f),3.1f,-10,cloudLayer);c2.color=new Color(1,.9f,.92f,.8f);clouds.Add(c2.transform);
   var motion=root.gameObject.AddComponent<AssetStageAtmosphere>();motion.clouds=clouds.ToArray();motion.speeds=new[]{.035f,.055f};
   EditorSceneManager.SaveScene(scene,R+"/LevelBackgroundDesign_Assets.unity");AssetDatabase.SaveAssets();
   Capture(camera,R+"/AssetStageDesign-Preview.png");
   File.WriteAllText(R+"/BuildReport.txt","SUCCESS: 3 original user building sprites, ready-made CraftPix ground, cobbled road and palms; GOBI Studio high-resolution clouds; native shader gradient sky. Existing scenes and build settings untouched.");Debug.Log("LAMIE_ASSET_STAGE_READY");Selection.activeGameObject=root.gameObject;
  }
  static Transform Group(string n){var t=new GameObject(n).transform;t.SetParent(root,false);t.gameObject.layer=29;return t;}
  static SpriteRenderer Sprite(string n,string path,Vector2 p,float height,int order,Transform parent)
  {var g=new GameObject(n);g.layer=29;g.transform.SetParent(parent,false);g.transform.localPosition=p;var r=g.AddComponent<SpriteRenderer>();r.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(R+"/Art/"+path);r.sharedMaterial=spriteMat;r.sortingOrder=order;g.transform.localScale=Vector3.one*(height/r.sprite.bounds.size.y);return r;}
  static Material Textured(string n,string path,Color wash){var m=new Material(Shader.Find("Lamie/Asset Stage Ground"));m.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(R+"/Art/"+path));m.SetColor("_WashColor",wash);AssetDatabase.CreateAsset(m,R+"/Materials/"+n+".mat");return m;}
  static void Mesh(string n,Vector3[] v,Vector2[] uv,int[] tri,Material m,int order,Transform parent)
  {var mesh=new UnityEngine.Mesh();mesh.name=n;mesh.vertices=v;mesh.uv=uv;mesh.triangles=tri;mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,R+"/Meshes/"+n+".asset");var g=new GameObject(n);g.layer=29;g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=m;r.sortingOrder=order;}
  static List<Vector2> Smooth(Vector2[] k){var p=new List<Vector2>();for(int i=0;i<k.Length-1;i++)for(int j=0;j<14;j++){float t=j/14f;var a=k[Mathf.Max(i-1,0)];var b=k[i];var c=k[i+1];var d=k[Mathf.Min(i+2,k.Length-1)];p.Add(.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}p.Add(k[k.Length-1]);return p;}
  static void Ribbon(string n,List<Vector2> p,float width,Material m,int order,Transform parent)
  {var v=new Vector3[p.Count*2];var uv=new Vector2[v.Length];var tri=new int[(p.Count-1)*6];float distance=0;for(int i=0;i<p.Count;i++){if(i>0)distance+=Vector2.Distance(p[i],p[i-1]);var tangent=(p[Mathf.Min(i+1,p.Count-1)]-p[Mathf.Max(i-1,0)]).normalized;var side=new Vector2(-tangent.y,tangent.x)*width*.5f;v[i*2]=p[i]-side;v[i*2+1]=p[i]+side;uv[i*2]=new Vector2(0,distance/1.6f);uv[i*2+1]=new Vector2(1,distance/1.6f);if(i<p.Count-1){int o=i*6,k=i*2;tri[o]=k;tri[o+1]=k+1;tri[o+2]=k+2;tri[o+3]=k+1;tri[o+4]=k+3;tri[o+5]=k+2;}}Mesh(n,v,uv,tri,m,order,parent);}
  public static void Capture(Camera c,string file){var rt=new RenderTexture(1920,1080,24);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1920,1080,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1920,1080),0,0);t.Apply();File.WriteAllBytes(file,t.EncodeToPNG());c.targetTexture=null;RenderTexture.active=old;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(t);}
 }
}


