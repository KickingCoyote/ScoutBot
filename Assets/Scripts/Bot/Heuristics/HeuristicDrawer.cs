using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HeuristicMenu))]
public class HeuristicDrawer : PropertyDrawer
{

    private HeuristicEditor editor = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Event guiEvent = Event.current;
        HeuristicMenu heuristicMenu = (HeuristicMenu)fieldInfo.GetValue(property.serializedObject.targetObject);

        float labelWidth = GUI.skin.label.CalcSize(label).x;
        Rect buttonRect = new Rect(position.x + 190, position.y, position.width - 190, position.height);

        if (guiEvent.type == EventType.Repaint)
        {
            GUIStyle style = new GUIStyle();
            GUI.Label(position, "Heuristics");

            style.normal.background = new Texture2D(1, 1);
            GUI.Label(buttonRect, GUIContent.none, style);
        }
        else if (guiEvent.type == EventType.MouseDown && buttonRect.Contains(guiEvent.mousePosition))
        {

            if (guiEvent.button == 0)
            {
                editor = EditorWindow.GetWindow<HeuristicEditor>();
                editor.SetMenu(heuristicMenu);
            }
            else if (editor != null && guiEvent.button == 1)
            {
                editor.Close();
            }


        }


    }
}
