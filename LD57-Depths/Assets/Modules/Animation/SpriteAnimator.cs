using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteAnimator : MonoBehaviour
{
    public static int FLIP_ID = Shader.PropertyToID("_FlipX");
    
    [Serializable]
    public class Clip
    {
        public string name;
        public Sprite[] frames;
        public float time = 0.2f;
        public bool randomStart = false;
    }

    public Clip[] states;
    public new SpriteRenderer renderer => _renderer ??= GetComponentInChildren<SpriteRenderer>();
    private SpriteRenderer _renderer;

    public Clip current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            _current = value;
            playTime = value.randomStart ? Random.value*value.time*value.frames.Length : 0f;
        }
    }
    private Clip _current;

    public void Play(string name) => current = states.FirstOrDefault(x => x.name == name);

    void Start()
    {
        current = states[0];
    }

    private float playTime = 0f;
    private void Update()
    {
        playTime += Time.deltaTime;
        var frame = Mathf.FloorToInt(playTime / current.time) % current.frames.Length;
        renderer.sprite = current.frames[frame];
    }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Clip))]
    public class ClipDrawer : PropertyDrawer
    {
        private Dictionary<string, Vector2> scrollPositions = new Dictionary<string, Vector2>();
        private SerializedProperty statesProp;

        private float cellWidth = 100f;
        private float cellHeight = 100f;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.x += 15;
            position.width -= 15;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(Clip.name)));
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(Clip.time)));
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(Clip.randomStart)));
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            var clipsProp = property.FindPropertyRelative(nameof(Clip.frames));
            
            position.height = EditorGUI.GetPropertyHeight(clipsProp);
            EditorGUI.PropertyField(position, clipsProp);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            position.height = cellHeight + GUI.skin.horizontalScrollbar.fixedHeight;
            
            float totalContentWidth = clipsProp.arraySize * cellWidth;
            
            string propertyPath = property.propertyPath;
            if (!scrollPositions.ContainsKey(propertyPath))
                scrollPositions[propertyPath] = Vector2.zero;
            
            scrollPositions[propertyPath] = GUI.BeginScrollView(position, scrollPositions[propertyPath],
                new Rect(position.x, position.y, totalContentWidth, position.height - GUI.skin.horizontalScrollbar.fixedHeight));
            for (int i = 0; i < clipsProp.arraySize; i++)
            {
                var sprite = clipsProp.GetArrayElementAtIndex(i).boxedValue as Sprite;
                if (!sprite) continue;
                var cellRect = new Rect(position.x + i * cellWidth, position.y, cellWidth, cellHeight);
                DrawTexturePreview(cellRect, sprite);
            }
            GUI.EndScrollView();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2f 
                   + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Clip.frames)))
                   + cellHeight + GUI.skin.horizontalScrollbar.fixedHeight;
        }

    }
    public static void DrawTexturePreview(Rect position, Sprite sprite)
    {
        Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
        Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);

        Rect coords = sprite.textureRect;
        coords.x /= fullSize.x;
        coords.width /= fullSize.x;
        coords.y /= fullSize.y;
        coords.height /= fullSize.y;

        Vector2 ratio;
        ratio.x = position.width / size.x;
        ratio.y = position.height / size.y;
        float minRatio = Mathf.Min(ratio.x, ratio.y);

        Vector2 center = position.center;
        position.width = size.x * minRatio;
        position.height = size.y * minRatio;
        position.center = center;

        GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
    }
    #endif
    
}
