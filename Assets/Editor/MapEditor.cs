using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor {
    
    bool autoUpdate = true;

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        MapGenerator map = target as MapGenerator;

        if( DrawDefaultInspector()&&autoUpdate ) {
            map.GenerateMap();
        }

        //if( autoUpdate && GUI.changed ) { 
        //    map.GenerateMap(); 
        //}

        if( GUILayout.Button( autoUpdate ? "Auto Update: ON" : "Auto Update: OFF" ) ) { 
            autoUpdate = !autoUpdate; 
        }

        if (GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }

}