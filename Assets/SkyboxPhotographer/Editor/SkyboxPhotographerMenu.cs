using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SkyboxPhotographerMenu : EditorWindow
{
    static private GUIStyle SkinBox = null;
    static private int ViewRange = 1000;
    static private int Resolution = 2048;

    /// <summary>
    /// NOTE : saved as static because Unity threading is weird and seem to destroy temporary variables on long operations
    /// </summary>
    static private Material Skybox;

    /// <summary>
    /// Task bar access
    /// </summary>
    [MenuItem("Skybox Photographer/Photograph !")]
    static void Photograph()
    {
        EditorWindow.GetWindow<SkyboxPhotographerMenu>().Show();
    }

    /// <summary>
    /// UI
    /// </summary>
    void OnGUI()
    {
        //Save original skin
        GUIStyle DefaultSkinBox = GUI.skin.box;
        if (SkyboxPhotographerMenu.SkinBox == null)
        {
            SkyboxPhotographerMenu.SkinBox = new GUIStyle(GUI.skin.box);
            SkyboxPhotographerMenu.SkinBox.fontStyle = FontStyle.BoldAndItalic;
            SkyboxPhotographerMenu.SkinBox.fontSize = 18;
        }
        GUI.skin.box = SkyboxPhotographerMenu.SkinBox;

        //Title
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box(" Skybox Photographer ");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(" v1.0 ");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //Restore skin for everyone else in Unity
        GUI.skin.box = DefaultSkinBox;

        //Checks
        GameObject Root = Selection.activeObject as GameObject;
        if (Root == null)
        {
            Texture2D Icon = (Texture2D)EditorGUIUtility.Load("icons/console.erroricon.png");
            GUIContent Message = new GUIContent("No viewpoint found.\r\nPlease select a GameObject in your scene. ", Icon, "Please fix the error to proceed.");
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(Message);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return;
        }

        //Reminder
        GUILayout.Space(10);
        GUILayout.Label("Current Viewpoint : " + Root.name);

        //Options
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("View Range :", GUILayout.Width(100));
        int.TryParse(GUILayout.TextField(SkyboxPhotographerMenu.ViewRange.ToString()), out SkyboxPhotographerMenu.ViewRange);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Resolution :", GUILayout.Width(100));
        int.TryParse(GUILayout.TextField(SkyboxPhotographerMenu.Resolution.ToString()), out SkyboxPhotographerMenu.Resolution);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        //Procede
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(" Photograph ! ", GUILayout.Width(200), GUILayout.Height(50)) == true)
        {
            SkyboxPhotographerMenu.Execute(Root, SkyboxPhotographerMenu.ViewRange, SkyboxPhotographerMenu.Resolution);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// Set up the cameras and components required to capture the skybox
    /// </summary>
    static public void Execute(GameObject Root, int ViewRange, int Resolution)
    {
        //Get a valid save path
        string Pathway = EditorUtility.SaveFilePanelInProject("Save viewpoint as Skybox", "New Skybox", "mat", "");
        if (Pathway.Length == 0) return;

        //Create required components
        GameObject Carrier = new GameObject();
        Carrier.name = "Temporary Photographer";
        Carrier.transform.SetParent(Root.transform);
        Carrier.transform.localPosition = Vector3.zero;
        Carrier.transform.localEulerAngles = Vector3.zero;
        Carrier.transform.localScale = Vector3.one;

        Camera[] Photographers = new Camera[6];
        RenderTexture[] RenderTextures = new RenderTexture[6];
        for (int Indice = 0; Indice < 6; Indice++)
        {
            GameObject Photographer = new GameObject();
            Photographer.transform.SetParent(Carrier.transform);
            Photographer.transform.localPosition = Vector3.zero;
            Photographer.transform.localScale = Vector3.one;

            //Orientation
            Vector3 Direction = Vector3.zero;
            string Name = "Forward";
            switch (Indice)
            {
                case 0:
                    Direction = new Vector3(0, 0, 0);
                    Name = "_FrontTex";
                    break;

                case 1:
                    Direction = new Vector3(0, 180, 0);
                    Name = "_BackTex";
                    break;

                case 2:
                    Direction = new Vector3(0, -90, 0);
                    Name = "_RightTex";
                    break;

                case 3:
                    Direction = new Vector3(0, 90, 0);
                    Name = "_LeftTex";
                    break;

                case 4:
                    Direction = new Vector3(-90, 0, 0);
                    Name = "_UpTex";
                    break;

                case 5:
                    Direction = new Vector3(90, 0, 0);
                    Name = "_DownTex";
                    break;

                default:
                    Debug.Log("Incorrect number of camera in SkyboxPhotographer.");
                    break;
            }
            Photographer.transform.localEulerAngles = Direction;
            Photographer.transform.name = Name;

            //Scene capture
            Camera ActualCamera = Photographer.AddComponent<Camera>();
            ActualCamera.fieldOfView = 90;
            ActualCamera.farClipPlane = ViewRange;
            Photographers[Indice] = ActualCamera;

            //Get view into texture
            RenderTexture RenderTexture = new RenderTexture(Resolution, Resolution, 24);
            RenderTexture.filterMode = FilterMode.Trilinear;
            RenderTexture.Create();
            ActualCamera.targetTexture = RenderTexture;
            RenderTextures[Indice] = RenderTexture;

            //Force rendering
            ActualCamera.Render();
        }

        //Get texture as assets
        Shader Shader = Shader.Find("Skybox/6 Sided");
        SkyboxPhotographerMenu.Skybox = new Material(Shader);
        for (int Indice = 0; Indice < 6; Indice++)
        {
            //Extract camera's view
            Rect Rectangle = new Rect(0, 0, Resolution, Resolution);
            Texture2D Intermediary = new Texture2D(Resolution, Resolution);
            RenderTexture.active = RenderTextures[Indice];
            Intermediary.ReadPixels(Rectangle, 0, 0);
            Intermediary.Apply();

            //Save as image
            byte[] PNG_RAW = Intermediary.EncodeToPNG();
            string TexName = Photographers[Indice].name;
            string TexturePathway = Pathway.Replace(".mat", TexName + ".png");
            File.WriteAllBytes(TexturePathway, PNG_RAW);

            //Import back as a texture and add to the material
            AssetDatabase.ImportAsset(TexturePathway, ImportAssetOptions.ForceUpdate);     //NOTE : Mandatory otherwise the AssetImporter won't find it
            TextureImporter ImportedPNG = (TextureImporter)AssetImporter.GetAtPath(TexturePathway);
            ImportedPNG.wrapMode = TextureWrapMode.Clamp;
            ImportedPNG.filterMode = FilterMode.Trilinear;
            Texture2D Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePathway);
            SkyboxPhotographerMenu.Skybox.SetTexture(TexName, Texture);
        }

        //Save the skybox itself
        AssetDatabase.CreateAsset(SkyboxPhotographerMenu.Skybox, Pathway);

        //Clean
        RenderTexture.active = null;
        GameObject.DestroyImmediate(Carrier);
    }

}
