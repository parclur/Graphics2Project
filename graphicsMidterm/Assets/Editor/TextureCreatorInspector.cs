using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Reference: https://catlikecoding.com/unity/tutorials/noise/
// Creates a custom inspector for our component so we can easily detect any change to our component; draws the default inspector and checks for changes. If a change happened while we are also in play mode, we should call FillTexture
[CustomEditor(typeof(TextureCreator))]
public class TextureCreatorInspector : Editor
{
    private TextureCreator creator; // stores a reference to the creator so we don't have to cast it every time

    private void OnEnable()
    {
        creator = target as TextureCreator;
        Undo.undoRedoPerformed += RefreshCreator;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= RefreshCreator;
    }

    private void RefreshCreator()
    {
        if (Application.isPlaying)
        {
            creator.FillTexture();
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            RefreshCreator();
        }
    }
}