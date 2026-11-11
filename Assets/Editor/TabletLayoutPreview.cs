using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Editor-only visual checks. No scene or gameplay data is saved by this tool.
public static class TabletLayoutPreview
{
    public static void Capture()
    {
        string folder = Argument("-lamiePreviewOutput");
        if (string.IsNullOrEmpty(folder)) throw new ArgumentException("Preview output folder is required.");
        CaptureSet(folder,Argument("-lamiePreviewSafe") == "true");
    }

    public static void CaptureBoth()
    {
        string folder = Argument("-lamiePreviewOutput");
        if (string.IsNullOrEmpty(folder)) throw new ArgumentException("Preview output folder is required.");
        CaptureSet(Path.Combine(folder,"normal"),false);
        CaptureSet(Path.Combine(folder,"additional-space"),true);
    }

    private static void CaptureSet(string folder,bool safeInsets)
    {
        Directory.CreateDirectory(folder);
        var log = new List<string>();
        foreach (var size in new[] { new Vector2Int(1024,768), new Vector2Int(1194,834), new Vector2Int(1280,800), new Vector2Int(1280,720) })
        foreach (string page in new[] { "frame", "design", "alignment", "question" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Lamie_Game/Features/Game_02/Game02Scene.unity", OpenSceneMode.Single);
            var objects = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            GameObject Find(string name) => objects.First(item => item.name == name).gameObject;
            Find("MuseumEntryContent").SetActive(false);
            Find("StoryTellingManager").SetActive(false);
            Find("Global_Fade").SetActive(false);
            Find("Task").SetActive(true);
            foreach (var step in new[] { "FrameChoose", "DesignTask", "PaintAllignment", "Question" })
                Find(step).SetActive(step == (page == "frame" ? "FrameChoose" : page == "design" ? "DesignTask" : page == "alignment" ? "PaintAllignment" : "Question"));
            foreach (var group in objects.Select(item => item.GetComponent<CanvasGroup>()).Where(item => item != null)) group.alpha = 1f;
            foreach (var rect in objects.Where(item => item.name == "SelectionFrame" || item.name == "ColorToolBox")) rect.gameObject.SetActive(false);
            var canvas = objects.Select(item => item.GetComponent<Canvas>()).First(item => item != null && item.isRootCanvas);
            var scaler = canvas.GetComponent<CanvasScaler>();
            Vector2 reference = scaler.referenceResolution;
            float sx = size.x / reference.x, sy = size.y / reference.y;
            float scale = scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand ? Mathf.Min(sx,sy) :
                Mathf.Pow(sx,1f - scaler.matchWidthOrHeight) * Mathf.Pow(sy,scaler.matchWidthOrHeight);
            scaler.enabled = false;
            var camera = objects.Select(item => item.GetComponent<Camera>()).First(item => item != null);
            var texture = new RenderTexture(size.x,size.y,24);
            camera.targetTexture = texture;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10f;
            canvas.scaleFactor = scale;
            Canvas.ForceUpdateCanvases();
            var rootRect = (RectTransform)canvas.transform;
            rootRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x / scale);
            rootRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y / scale);
            var fit = objects.Select(item => item.GetComponent<TabletSafeArea>()).FirstOrDefault(item => item != null);
            Rect usable = rootRect.rect;
            if (safeInsets) usable = new Rect(usable.xMin + 24f / scale, usable.yMin + 20f / scale,
                usable.width - 48f / scale, usable.height - 40f / scale);
            if (fit != null) fit.ApplyLayout(usable);
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var old = RenderTexture.active;
            RenderTexture.active = texture;
            var image = new Texture2D(size.x,size.y,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);
            image.Apply();
            File.WriteAllBytes(Path.Combine(folder,page+"-"+size.x+"x"+size.y+".png"),image.EncodeToPNG());
            foreach (var button in objects.Select(item => item.GetComponent<Button>()).Where(item => item != null && item.gameObject.activeInHierarchy))
            {
                string name = button.name;
                if (!(name.Contains("Next") || name.Contains("Confirm") || name.EndsWith("_Tag") || name == "HappyEid" || name == "SadEid" || name == "CalmDay")) continue;
                var corners = new Vector3[4];
                ((RectTransform)button.transform).GetWorldCorners(corners);
                Vector3 low = camera.WorldToScreenPoint(corners[0]);
                Vector3 high = camera.WorldToScreenPoint(corners[2]);
                bool inside = low.x >= -1f && low.y >= -1f && high.x <= size.x+1 && high.y <= size.y+1;
                log.Add(page+" "+size+" "+name+" inside="+inside+" bounds="+low+" to "+high);
            }
            RenderTexture.active = old;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(texture);
        }
        File.WriteAllLines(Path.Combine(folder,"button-bounds.txt"),log);
        Debug.Log("Tablet previews captured: "+folder);
    }

    private static string Argument(string name)
    {
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args,name);
        return index >= 0 && index + 1 < args.Length ? args[index+1] : null;
    }
}
