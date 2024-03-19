using System.IO;
using System.Linq;
using Legacy;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class GameMenu : EditorWindow
{
    [MenuItem("Game/Take screenshot")]
    public static void TakeScreenshot()
    {
        string path = "Screenshots";

        Directory.CreateDirectory(path);

        int i = 0;

        while (File.Exists($"{path}/{i}.png"))
        {
            i++;
        }

        ScreenCapture.CaptureScreenshot($"{path}/{i}.png");
    }

    [MenuItem("Game/Tools/Pos all trees")]
    public static void PositionAllTrees()
    {
        Torch[] trees = FindObjectsOfType<Torch>();

        Undo.RecordObjects(trees.Select(t => t.transform).ToArray(), "pos all trees");

        foreach (Torch tree in trees)
            tree.Positioning(false);
    }

    [MenuItem("Game/Check current scene for Standard material")]
    private static void CheckMaterials()
    {
        bool found = false;

        foreach (MeshRenderer renderer in FindObjectsOfType<MeshRenderer>())
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material.shader.name != "Standard")
                    continue;

                Debug.LogError(material.shader.name, renderer);
                found = true;
            }
        }

        if (!found)
            Debug.Log("Well done, no any Standard materials on current scene!");
    }
}
#endif