using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( MapGenerator ) )]
public class MapEditor: Editor {

    private bool autoUpdate = false;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        MapGenerator map = target as MapGenerator;

        if( GUILayout.Button( "Generate Map" ) ) {
            map.GenerateMap();
        }

        if( GUILayout.Button( autoUpdate ? "Auto Update: ON" : "Auto Update: OFF" ) ) {
            autoUpdate = !autoUpdate;
        }

        if( autoUpdate && GUI.changed ) {
            map.GenerateMap();
        }
    }
}
