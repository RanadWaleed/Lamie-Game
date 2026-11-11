using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace Lamie.NativeStageDesign
{
    public static class NativeStageDesignBuilder
    {
        const string Root="Assets/Lamie_Game/DesignPreviews/UnityNativeStageDesign";
        static Transform world;
        static Dictionary<string,Material> mats;
        static List<Transform> clouds, twinkles;
        [InitializeOnLoadMethod]
        static void QueueRequestedBuild()
        {
            if (!File.Exists(Root+"/BuildRequested.txt") || File.Exists(Root+"/LevelBackgroundDesign_Native.unity")) return;
            EditorApplication.delayCall += () => { if(!EditorApplication.isPlayingOrWillChangePlaymode) Build(); };
        }
        static Color Hex(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
        static Material M(string name,string color)
        {
            if(mats.TryGetValue(name,out var m))return m;
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            m=new Material(shader);m.name=name;
            m.color=Hex(color);if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",Hex(color));
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.18f);
            AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat");mats[name]=m;return m;
        }
        static Transform Group(string name,Transform parent=null)
        {var g=new GameObject(name).transform;g.SetParent(parent?parent:world,false);return g;}
        static Transform P(string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material mat,Transform parent)
        {
            var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);
            g.transform.localPosition=pos;g.transform.localScale=scale;
            g.GetComponent<Renderer>().sharedMaterial=mat;
            Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;
        }
        static Transform Box(string n,Vector3 p,Vector3 s,Material m,Transform t)=>P(n,PrimitiveType.Cube,p,s,m,t);
        static Transform Ball(string n,Vector3 p,Vector3 s,Material m,Transform t)=>P(n,PrimitiveType.Sphere,p,s,m,t);
        static Transform Disc(string n,Vector3 p,float r,float h,Material m,Transform t)=>P(n,PrimitiveType.Cylinder,p,new Vector3(r*2,h/2,r*2),m,t);
        static Transform Rod(string n,Vector3 a,Vector3 b,float radius,Material m,Transform t)
        {var v=b-a;var p=P(n,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(radius*2,v.magnitude/2,radius*2),m,t);p.localRotation=Quaternion.FromToRotation(Vector3.up,v);return p;}

        [MenuItem("Lamie/Design Previews/Build Native Stage Background")]
        public static void Build()
        {
            string target=Root+"/LevelBackgroundDesign_Native.unity";
            if(File.Exists(target)){Debug.Log("Native stage scene already exists. No overwrite performed.");return;}
            Directory.CreateDirectory(Root+"/Materials");
            Directory.CreateDirectory(Root+"/Meshes");AssetDatabase.Refresh();
            mats=new Dictionary<string,Material>();clouds=new List<Transform>();twinkles=new List<Transform>();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
            world=new GameObject("LAMIE - Native Editable Stage Design").transform;
            var sand=M("Sandstone","#F4CBA0");var cream=M("PathCream","#FFE5B3");
            var gold=M("WarmGold","#E8AD39");var turquoise=M("Turquoise","#138B90");
            var plum=M("Plum","#82458F");var terra=M("Terracotta","#D87845");
            var wood=M("CarvedWood","#824B39");var dark=M("DoorShadow","#49344F");
            var green=M("PalmGreen","#648B55");var lightgreen=M("LeafLight","#9DBA65");
            var pink=M("Bougainvillea","#D94B8C");var lavender=M("CourtyardLavender","#B8A0B7");
            var cloudMat=M("PeachCloud","#FFE1D6");var glow=M("LanternWarmth","#FFE29A");
            var ground=Group("01 - Ground and Geometric Courtyard");
            Box("Courtyard foundation",new Vector3(0,-.35f,4),new Vector3(29,.7f,23),wood,ground);
            Box("Lavender paving",new Vector3(0,.01f,4),new Vector3(28.7f,.12f,22.7f),lavender,ground);
            // Editable stone grid, subtly alternating tones.
            var tile=M("TileHighlight","#C5ACBC");
            for(int x=-13;x<=13;x+=2)for(int z=-6;z<=14;z+=2)
                Box("Paving stone",new Vector3(x,.09f,z),new Vector3(1.94f,.035f,1.94f),(x+z)%4==0?tile:lavender,ground);
            var road=Group("02 - Curved Walkway - Individual Native Mesh");
            var points=new List<Vector3>();
            for(int i=0;i<=110;i++)
            {float u=i/110f;points.Add(new Vector3(Mathf.Cos(u*Mathf.PI*2.15f)*5.4f,.17f,-5+u*18));}
            Ribbon("Gold path border",points,2.65f,gold,road);
            var raised=new List<Vector3>();foreach(var p in points)raised.Add(p+Vector3.up*.025f);
            Ribbon("Cream walking surface",raised,2.38f,cream,road);
            var stations=Group("03 - Three Independent Stage Buildings");
            Building("01 - Orange Market",new Vector3(5.1f,0,-3.5f),terra,0,stations);
            Building("02 - Turquoise Library",new Vector3(-4.6f,0,3.1f),turquoise,1,stations);
            Building("03 - Purple Art House",new Vector3(4.8f,0,10.2f),plum,2,stations);
            var surroundings=Group("04 - Hijazi Architecture and Rawasheen");
            foreach(int side in new[]{-1,1})for(int i=0;i<3;i++)
            {
                var g=Group("Hijazi house "+side+" - "+i,surroundings);g.localPosition=new Vector3(side*12.6f,0,i*6-2);
                float h=4.2f+i*.65f;Box("Warm stone house",new Vector3(0,h/2,0),new Vector3(3,h,3.5f),sand,g);
                Box("Roof cornice",new Vector3(0,h,0),new Vector3(3.35f,.28f,3.8f),cream,g);
                Balcony(g,new Vector3(0,h*.65f,-1.95f),turquoise,wood,gold);
                Arch(g,new Vector3(0,0,-1.79f),1.2f,2.4f,wood,dark,cream);
            }
            var plants=Group("05 - Palms Planters and Flowers");
            foreach(var p in new[]{new Vector3(-10,0,-5),new Vector3(10,0,0),new Vector3(-10,0,7),new Vector3(10,0,13),new Vector3(-7,0,13)})Palm(p,plants);
            foreach(var p in new[]{new Vector3(-9,0,-4),new Vector3(9,0,-5),new Vector3(-9,0,3),new Vector3(9,0,7),new Vector3(-3,0,12)})
            {
                var pot=Group("Terracotta flower planter",plants);pot.localPosition=p;
                Disc("Pot",new Vector3(0,.4f,0),.5f,.8f,terra,pot);Disc("Gold lip",new Vector3(0,.8f,0),.56f,.14f,gold,pot);
                for(int j=0;j<7;j++){float a=j*Mathf.PI*2/7;var v=new Vector3(Mathf.Cos(a)*.4f,1+Mathf.Sin(j)*.12f,Mathf.Sin(a)*.4f);Ball("Leaf cluster",v,new Vector3(.5f,.5f,.5f),green,pot);Ball("Pink blossom",v+Vector3.up*.2f,Vector3.one*.3f,pink,pot);}
            }
            var sky=Group("06 - Clouds - Individual Animated Objects");
            for(int i=0;i<4;i++)
            {
                var c=Group("Cloud group "+(i+1),sky);c.localPosition=new Vector3(-10+i*6.5f,7+i%2,16+i%2);
                for(int j=0;j<5;j++)Ball("Cloud puff "+j,new Vector3((j-2)*.75f,j==2?.2f:0,0),new Vector3(1.65f,j==2?1.15f:.65f,.8f),cloudMat,c);
                clouds.Add(c);
            }
            var details=Group("07 - Lanterns and Subtle Light Motes");
            for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++)
            {
                var l=Group("Lantern post",details);l.localPosition=new Vector3(side*9,0,-5+i*6);
                Rod("Post",Vector3.zero,new Vector3(0,2.8f,0),.065f,wood,l);
                Box("Lantern glowing glass",new Vector3(0,2.55f,0),new Vector3(.38f,.6f,.38f),glow,l);
                Box("Lantern cap",new Vector3(0,2.9f,0),new Vector3(.56f,.12f,.56f),gold,l);
                Box("Lantern base",new Vector3(0,2.2f,0),new Vector3(.5f,.1f,.5f),gold,l);
                for(int j=-1;j<=1;j+=2)for(int k=-1;k<=1;k+=2)Rod("Lantern frame",new Vector3(j*.2f,2.2f,k*.2f),new Vector3(j*.2f,2.9f,k*.2f),.025f,wood,l);
            }
            var rng=new System.Random(19);for(int i=0;i<18;i++)twinkles.Add(Ball("Floating light mote",new Vector3((float)rng.NextDouble()*19-9.5f,.7f+(float)rng.NextDouble()*2,-3+(float)rng.NextDouble()*15),Vector3.one*.12f,glow,details));
            var animation=world.gameObject.AddComponent<NativeStageMotion>();animation.clouds=clouds.ToArray();animation.lights=twinkles.ToArray();
            var sun=new GameObject("Warm evening sun").AddComponent<Light>();sun.type=LightType.Directional;sun.color=Hex("#FFE6C3");sun.intensity=1.6f;sun.transform.rotation=Quaternion.Euler(45,-35,0);sun.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=Hex("#C6B6DB");
            var camera=new GameObject("Native Design Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=13.4f;
            camera.transform.position=new Vector3(0,22,-29);camera.transform.LookAt(new Vector3(0,1.4f,4));camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Hex("#C9A8D9");camera.nearClipPlane=.1f;camera.farClipPlane=150;
            EditorSceneManager.SaveScene(scene,target);AssetDatabase.SaveAssets();
            Capture(camera);
            Selection.activeGameObject=world.gameObject;SceneView.RepaintAll();
            File.WriteAllText(Root+"/BuildReport.txt","SUCCESS: native Unity geometry. No generated background image, no existing scenes edited. Clouds: 4. Stage buildings: 3. Scene: "+target);
            Debug.Log("LAMIE_NATIVE_DESIGN_SUCCESS "+target);
        }
        static void Ribbon(string name,List<Vector3> pts,float width,Material mat,Transform parent)
        {
            var verts=new Vector3[pts.Count*2];var uv=new Vector2[verts.Length];var triangles=new int[(pts.Count-1)*6];
            for(int i=0;i<pts.Count;i++){var tangent=pts[Mathf.Min(i+1,pts.Count-1)]-pts[Mathf.Max(i-1,0)];var side=Vector3.Cross(tangent.normalized,Vector3.up)*width*.5f;verts[i*2]=pts[i]-side;verts[i*2+1]=pts[i]+side;uv[i*2]=new Vector2(0,i);uv[i*2+1]=new Vector2(1,i);if(i<pts.Count-1){int o=i*6,k=i*2;triangles[o]=k;triangles[o+1]=k+2;triangles[o+2]=k+1;triangles[o+3]=k+1;triangles[o+4]=k+2;triangles[o+5]=k+3;}}
            for(int i=0;i<triangles.Length;i+=3){int swap=triangles[i+1];triangles[i+1]=triangles[i+2];triangles[i+2]=swap;}
            var mesh=new Mesh();mesh.name=name;mesh.vertices=verts;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,Root+"/Meshes/"+name+".asset");var g=Group(name,parent);g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;
        }
        static void Arch(Transform parent,Vector3 p,float w,float h,Material frame,Material interior,Material trim)
        {
            var g=Group("Arched entrance - editable voussoirs",parent);g.localPosition=p;
            Box("Door shadow",new Vector3(0,h*.42f,0),new Vector3(w,h*.84f,.15f),interior,g);
            Box("Left jamb",new Vector3(-w*.58f,h*.32f,-.08f),new Vector3(.2f,h*.7f,.22f),frame,g);
            Box("Right jamb",new Vector3(w*.58f,h*.32f,-.08f),new Vector3(.2f,h*.7f,.22f),frame,g);
            for(int i=0;i<=10;i++){float a=i*Mathf.PI/10;var b=Box("Arch stone "+i,new Vector3(Mathf.Cos(a)*w*.57f,h*.65f+Mathf.Sin(a)*w*.57f,-.09f),new Vector3(.24f,.27f,.27f),trim,g);b.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg-90);}
        }
        static void Balcony(Transform parent,Vector3 p,Material color,Material wood,Material gold)
        {
            var g=Group("Rawshan carved balcony",parent);g.localPosition=p;
            Box("Window recess",Vector3.zero,new Vector3(1.5f,1.5f,.25f),wood,g);
            Box("Balcony sill",new Vector3(0,-.7f,-.22f),new Vector3(1.9f,.15f,.7f),color,g);
            Box("Window top",new Vector3(0,.8f,-.15f),new Vector3(1.85f,.18f,.55f),color,g);
            for(int i=-3;i<=3;i++)Box("Carved vertical lattice",new Vector3(i*.23f,0,-.3f),new Vector3(.055f,1.5f,.08f),color,g);
            for(int i=-2;i<=2;i++)Box("Carved horizontal lattice",new Vector3(0,i*.29f,-.35f),new Vector3(1.65f,.045f,.08f),gold,g);
        }
        static void Building(string name,Vector3 p,Material accent,int kind,Transform parent)
        {
            var g=Group(name,parent);g.localPosition=p;
            Disc("Gold station rim",new Vector3(0,.23f,-.7f),2,.32f,mats["WarmGold"],g);
            Disc("Turquoise mosaic station",new Vector3(0,.42f,-.7f),1.84f,.12f,mats["Turquoise"],g);
            for(int j=0;j<12;j++){float a=j*Mathf.PI/6;var t=Box("Mosaic inlay",new Vector3(Mathf.Cos(a)*1.55f,.49f,-.7f+Mathf.Sin(a)*1.55f),new Vector3(.13f,.02f,.32f),mats["PathCream"],g);t.localRotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg,0);}
            Box("Stone facade",new Vector3(0,1.8f,.65f),new Vector3(3.4f,3.2f,2),mats["Sandstone"],g);
            Box("Roof parapet",new Vector3(0,3.5f,.65f),new Vector3(3.75f,.23f,2.25f),accent,g);
            Box("Upper gold trim",new Vector3(0,3.7f,.65f),new Vector3(3.9f,.12f,2.4f),mats["WarmGold"],g);
            Arch(g,new Vector3(0,.5f,-.42f),1.25f,2.4f,accent,mats["DoorShadow"],mats["PathCream"]);
            Box("Doorstep",new Vector3(0,.35f,-.7f),new Vector3(1.65f,.18f,.7f),mats["PathCream"],g);
            for(int side=-1;side<=1;side+=2)Box("Facade color pier",new Vector3(side*1.53f,1.75f,-.4f),new Vector3(.22f,3.1f,.22f),accent,g);
            var medallion=Disc("Roof badge",new Vector3(0,4.1f,.0f),.62f,.17f,mats["WarmGold"],g);medallion.localRotation=Quaternion.Euler(90,0,0);
            if(kind==0)
            {
                for(int j=0;j<8;j++){var aw=Box("Striped market awning",new Vector3(-1.55f+j*.44f,2.9f,-.9f),new Vector3(.44f,.12f,1.25f),j%2==0?accent:mats["PathCream"],g);aw.localRotation=Quaternion.Euler(-15,0,0);}
                for(int j=0;j<3;j++)Box("Market crate",new Vector3(-2.0f,.65f+j*.43f,-.2f),new Vector3(.7f,.4f,.7f),mats["CarvedWood"],g);
            }
            if(kind==1)
            {
                for(int side=-1;side<=1;side+=2){Box("Book shelf",new Vector3(side*1.14f,1.2f,-.52f),new Vector3(.65f,1.2f,.3f),mats["CarvedWood"],g);for(int j=0;j<4;j++)Box("Colorful book",new Vector3(side*1.14f-.21f+j*.14f,1.3f,-.72f),new Vector3(.1f,.63f,.2f),j%2==0?mats["Plum"]:mats["WarmGold"],g);}
                var book1=Box("Open book left page",new Vector3(-.2f,4.1f,-.19f),new Vector3(.36f,.48f,.08f),mats["PathCream"],g);book1.localRotation=Quaternion.Euler(0,0,8);
                var book2=Box("Open book right page",new Vector3(.2f,4.1f,-.19f),new Vector3(.36f,.48f,.08f),mats["PathCream"],g);book2.localRotation=Quaternion.Euler(0,0,-8);
            }
            if(kind==2){for(int j=0;j<3;j++)Ball("Palette color",new Vector3((j-1)*.23f,4.15f,-.2f),Vector3.one*.18f,j==0?mats["Bougainvillea"]:j==1?mats["Turquoise"]:mats["PathCream"],g);}
            // Native text mesh; numbers only, no font or RTL dependency.
            var label=new GameObject("Stage number "+(kind+1)).AddComponent<TextMesh>();label.transform.SetParent(g,false);label.transform.localPosition=new Vector3(0,.55f,-2.12f);label.transform.localRotation=Quaternion.Euler(35,0,0);label.text=(kind+1).ToString();label.fontSize=80;label.characterSize=.14f;label.anchor=TextAnchor.MiddleCenter;label.color=mats["DoorShadow"].color;
        }
        static void Palm(Vector3 p,Transform parent)
        {
            var g=Group("Palm - trunk and separate fronds",parent);g.localPosition=p;
            Rod("Palm trunk",Vector3.zero,new Vector3(.3f,4,0),.17f,mats["CarvedWood"],g);
            for(int j=0;j<7;j++)Disc("Trunk ring",new Vector3(j*.3f/7,j*.55f,0),.19f,.08f,mats["Terracotta"],g);
            for(int j=0;j<9;j++){float a=j*Mathf.PI*2/9;Vector3 start=new Vector3(.3f,4,0);Vector3 mid=start+new Vector3(Mathf.Cos(a)*1.2f,.4f,Mathf.Sin(a)*1.2f);Vector3 end=start+new Vector3(Mathf.Cos(a)*2.1f,-.4f,Mathf.Sin(a)*2.1f);var leaf=Ball("Palm frond",(start+mid)*.5f,new Vector3(.45f,(mid-start).magnitude,.15f),mats["PalmGreen"],g);leaf.localRotation=Quaternion.FromToRotation(Vector3.up,mid-start);var tip=Ball("Frond tip",(mid+end)*.5f,new Vector3(.35f,(end-mid).magnitude,.12f),mats["LeafLight"],g);tip.localRotation=Quaternion.FromToRotation(Vector3.up,end-mid);}
        }
        static void Capture(Camera camera)
        {
            var rt=new RenderTexture(1920,1080,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
            var t=new Texture2D(1920,1080,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1920,1080),0,0);t.Apply();File.WriteAllBytes(Root+"/NativeStageDesign-Preview.png",t.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(t);
        }
    }
}
