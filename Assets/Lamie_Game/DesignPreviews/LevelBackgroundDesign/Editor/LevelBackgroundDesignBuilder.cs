using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lamie.DesignPreview
{
    public static class LevelBackgroundDesignBuilder
    {
        const string Root = "Assets/Lamie_Game/DesignPreviews/LevelBackgroundDesign";

        [MenuItem("Lamie/Design Previews/Create Level Background Design")]
        public static void Build()
        {
            string target = Root + "/LevelBackgroundDesign.unity";
            if (File.Exists(target)) throw new IOException("Design scene already exists; refusing to overwrite.");
            AssetDatabase.Refresh();
            var importer = (TextureImporter)AssetImporter.GetAtPath(Root + "/CourtyardBackground.png");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            Sprite backdrop = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/CourtyardBackground.png");

            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var cameraObject = new GameObject("Design Camera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.19f, 0.1f, 0.26f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100;
            camera.allowHDR = false;

            var world = new GameObject("Level Background Design - Independent Preview");
            var background = new GameObject("01 - Painted Courtyard and Ground").AddComponent<SpriteRenderer>();
            background.transform.SetParent(world.transform);
            background.sprite = backdrop;
            background.transform.localScale = Vector3.one * (10 / backdrop.bounds.size.y);
            background.sortingOrder = 0;

            Sprite puff = MakeSprite("CloudPuff", false);
            Sprite glow = MakeSprite("SoftGlow", true);
            var cloudRoot = new GameObject("02 - Animated Sky Clouds");
            cloudRoot.transform.SetParent(world.transform);
            var clouds = new List<Transform>();
            float[] xs = { -3.3f, 0.2f, 3.4f };
            float[] ys = { 3.35f, 4.0f, 3.05f };
            for (int i = 0; i < 3; i++)
            {
                var cloud = new GameObject("Cloud " + (i + 1));
                cloud.transform.SetParent(cloudRoot.transform);
                cloud.transform.localPosition = new Vector3(xs[i], ys[i], 0);
                float scale = i == 1 ? 0.65f : 0.85f;
                cloud.transform.localScale = Vector3.one * scale;
                for (int j = 0; j < 5; j++)
                {
                    var sr = new GameObject("Painted puff " + j).AddComponent<SpriteRenderer>();
                    sr.transform.SetParent(cloud.transform);
                    sr.transform.localPosition = new Vector3((j - 2) * 0.42f, j == 2 ? 0.14f : 0, 0);
                    sr.transform.localScale = new Vector3(j == 2 ? 1.15f : 1, j == 2 ? 0.55f : 0.35f, 1);
                    sr.sprite = puff;
                    sr.color = new Color(1f, 0.79f, 0.88f, 0.68f);
                    sr.sortingOrder = 2;
                }
                clouds.Add(cloud.transform);
            }
            var effects = new GameObject("03 - Fireflies and Lantern Light");
            effects.transform.SetParent(world.transform);
            var motes = new List<SpriteRenderer>();
            var random = new System.Random(27);
            for (int i = 0; i < 24; i++)
            {
                var sr = new GameObject("Warm mote " + (i + 1)).AddComponent<SpriteRenderer>();
                sr.transform.SetParent(effects.transform);
                sr.transform.localPosition = new Vector3((float)random.NextDouble() * 13 - 6.5f, (float)random.NextDouble() * 4 - 3.4f, 0);
                sr.transform.localScale = Vector3.one * (0.035f + (float)random.NextDouble() * 0.04f);
                sr.sprite = glow;
                sr.color = new Color(1, 0.85f, 0.43f, 0.45f);
                sr.sortingOrder = 4;
                motes.Add(sr);
            }
            var lights = new List<SpriteRenderer>();
            foreach (Vector2 p in new[] { new Vector2(-6.25f, 0.65f), new Vector2(7.85f, 0.85f) })
            {
                var sr = new GameObject("Lantern breathing glow").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(effects.transform);
                sr.transform.localPosition = p;
                sr.transform.localScale = Vector3.one * 1.15f;
                sr.sprite = glow;
                sr.color = new Color(1f, 0.66f, 0.17f, 0.12f);
                sr.sortingOrder = 3;
                lights.Add(sr);
            }
            var motion = world.AddComponent<LevelBackgroundAtmosphere>();
            motion.clouds = clouds.ToArray();
            motion.sparkles = motes.ToArray();
            motion.lanternGlows = lights.ToArray();
            EditorSceneManager.SaveScene(scene, target);
            AssetDatabase.SaveAssets();
            RenderTexture rt = new RenderTexture(1920, 1080, 24);
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture.active = rt;
            var shot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            shot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            shot.Apply();
            File.WriteAllBytes(Root + "/LevelBackgroundDesign-Preview.png", shot.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(shot);
            rt.Release();
            Object.DestroyImmediate(rt);
            Debug.Log("LAMIE_DESIGN_SUCCESS: " + target + "; three cloud groups; 24 animated motes; two breathing glows. Existing scenes and build settings untouched.");
            EditorSceneManager.CloseScene(scene, true);
            if (previous.IsValid()) SceneManager.SetActiveScene(previous);
        }

        static Sprite MakeSprite(string name, bool soft)
        {
            const int n = 128;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                float a = soft ? Mathf.Pow(Mathf.Clamp01(1 - d), 2) : 1 - Mathf.SmoothStep(0.94f, 1, d);
                t.SetPixel(x, y, new Color(1, 1, 1, a));
            }
            t.Apply();
            string path = Root + "/" + name + ".png";
            File.WriteAllBytes(path, t.EncodeToPNG());
            Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = n;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
