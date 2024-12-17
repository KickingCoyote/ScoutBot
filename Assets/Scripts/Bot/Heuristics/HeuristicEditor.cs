using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HeuristicEditor : EditorWindow
{

    private HeuristicMenu menu;

    private int borderSize = 10;

    public void SetMenu(HeuristicMenu heuristicMenu)
    {
        this.menu = heuristicMenu;
    }

    

    public void OnGUI()
    {
        Rect rect = new Rect(borderSize, borderSize, position.width - 2 * borderSize, position.height - 2 * borderSize);
        GUILayout.BeginArea(rect);

        for (int i = 1; i < 5; i++)
        {
            SBH h = menu.GetHeuristic(i);
            string id = menu.GetId(i);

            EditorGUILayout.LabelField("Player " + i);
            menu.SetHeuristic(i, (SBH)EditorGUILayout.ObjectField(h, typeof(SBH), true));

            menu.SetId(i, EditorGUILayout.TextField(id));
            GUILayout.Space(borderSize);
        }

        GUILayout.EndArea();
    }
    

    public void OnEnable()
    {
        titleContent.text = "Heuristic Editor";
    }
}
