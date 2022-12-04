#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ScreenshotGrabber
{
    [MenuItem("Screenshot/Grab")]
    public static void Grab()
    {
        var path = EditorUtility.SaveFilePanel(
                    "Save Screenshot",
                    "",
                    "Screenshot.png",
                    "png");

        ScreenCapture.CaptureScreenshot(path, 1);
        Debug.Log($"Screenshot store to {path}");
    }
}
#endif