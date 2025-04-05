using System;
using System.Collections.Generic;
using System.IO;
using elZach.Common;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PaintingCanvas : MonoBehaviour
{
    public Texture2D tex;
    public LayerMask paintingLayer = 1 << 1;

    public InputActionReference paintingAction;
    public InputActionReference eraseAction;
    public InputActionReference pointerPosition;
    
    public Button clearButton;
    public Button[] paintButtons;
    
    public Color currentColor { get; set; }


    [Serializable]
    public class Palette
    {
        public Color[] colors;
    }

    void Start()
    {
        currentColor = paintButtons[0].GetComponent<Image>()!.color;

        for (int i = 0; i < paintButtons.Length; i++)
        {
            var color = paintButtons[i].GetComponent<Image>()!.color;
            paintButtons[i].onClick.AddListener(() => currentColor = color);
        }

        clearButton.onClick.AddListener(() =>
        {
            FillColor(Color.clear);
        });
        FillColor(Color.clear);
    }
    
    void Update()
    {
        if (paintingAction.action.IsPressed()) DrawColor(currentColor);
        else if (eraseAction.action.IsPressed()) DrawColor(Color.clear);
        
    }

    private static Camera cam;
    void DrawColor(Color col)
    {
        if (!cam) cam = Camera.main;
        var ray = cam.ScreenPointToRay(pointerPosition.action.ReadValue<Vector2>());
        if (Physics.Raycast(ray, out var hit, 100f, paintingLayer))
        {
            var coord = hit.textureCoord;
            
            int x = Mathf.FloorToInt(coord.x * tex.width);
            int y = Mathf.FloorToInt(coord.y * tex.height);

            // Ensure pixel coordinates are within texture bounds
            x = Mathf.Clamp(x, 0, tex.width - 1);
            y = Mathf.Clamp(y, 0, tex.height - 1);

            // Set the pixel color to white
            tex.SetPixel(x, y, col);
            tex.Apply();
        }
    }

    void FillColor(Color col)
    {
        var cols = new Color[32 * 32];
        for (int i = 0; i < cols.Length; i++) cols[i] = col;
        tex.SetPixels(0,0,32,32, cols);
        tex.Apply();
    }
    
    public void ChangePalette(Color[] oldColors, Color[] newColors)
    {
        if (oldColors.Length != newColors.Length)
        {
            Debug.LogError("OldColors and NewColors arrays must have the same length.");
            return;
        }
        Color[] pixels = tex.GetPixels();
        
        for (int i = 0; i < pixels.Length; i++)
        {
            for (int j = 0; j < oldColors.Length; j++)
            {
                if (!AreColorsEqual(pixels[i], oldColors[j])) continue;
                pixels[i] = newColors[j];
                break;
            }
        }

        // Set the modified pixels back to the texture
        tex.SetPixels(pixels);
        tex.Apply();
    }

    // Helper function to compare colors with a tolerance
    private bool AreColorsEqual(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }
    
    public static string SerializeTextureToBase64(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("SerializeTextureToBase64: texture is null.");
            return null;
        }
        byte[] bytes = texture.EncodeToPNG();
        string base64String = Convert.ToBase64String(bytes);

        return base64String;
    }
    
    public static Texture2D DeserializeTextureFromBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            Debug.LogError("DeserializeTextureFromBase64: base64String is null or empty.");
            return null;
        }

        byte[] bytes = Convert.FromBase64String(base64String);

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        bool isLoaded = texture.LoadImage(bytes);

        if (!isLoaded)
        {
            Debug.LogError("DeserializeTextureFromBase64: Failed to load texture from Base64 string.");
            return null;
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        return texture;
    }
    
    #if UNITY_EDITOR
    [SerializeField] private Button<PaintingCanvas> saveToDiskButton = new Button<PaintingCanvas>(x => x.SaveTextureToDisk());
    [SerializeField] private Button<PaintingCanvas> serializeToB64Button = new Button<PaintingCanvas>(x => Debug.Log(SerializeTextureToBase64(x.tex)));
    public void SaveTextureToDisk()
    {
        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();

        // Get the path to save the file
        string directoryPath = Application.persistentDataPath + "/SavedTextures";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Create a unique file name
        string fileName = "Texture_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        string filePath = Path.Combine(directoryPath, fileName);

        // Write the PNG file to disk
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Texture saved to: " + filePath);
    }    
    #endif
}
