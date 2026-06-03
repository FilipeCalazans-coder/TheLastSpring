using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ToxicResinShape))]
public class ToxicResinShapeEditor : Editor
{
    private const float HANDLE_SIZE = 0.12f;
    private const float MIDPOINT_HANDLE_SIZE = 0.08f;

    private void OnSceneGUI()
    {
        ToxicResinShape shape = (ToxicResinShape)target;
        if (shape == null || shape.Points == null || shape.Points.Count < 3) return;

        Transform t = shape.transform;
        var points = shape.Points;

        Event e = Event.current;
        bool shiftHeld = e.shift;
        bool ctrlHeld = e.control || e.command;

        // === 1. Handles dos vértices existentes ===
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 worldPos = t.TransformPoint(new Vector3(points[i].x, points[i].y, 0f));

            // Cor: cyan normal, vermelho se Ctrl pressionado (modo deletar)
            Handles.color = ctrlHeld ? Color.red : Color.cyan;

            float handleSize = HandleUtility.GetHandleSize(worldPos) * HANDLE_SIZE;

            // Botão clicável pra deletar (com Ctrl)
            if (ctrlHeld)
            {
                if (Handles.Button(worldPos, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
                {
                    if (points.Count > 3)
                    {
                        Undo.RecordObject(shape, "Remove Resin Point");
                        shape.RemovePoint(i);
                        EditorUtility.SetDirty(shape);
                        return;
                    }
                    else
                    {
                        Debug.LogWarning("Mínimo de 3 pontos no polígono.");
                    }
                }
            }
            else
            {
                // Handle de movimento
                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPos = Handles.FreeMoveHandle(
                    worldPos,
                    handleSize,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(shape, "Move Resin Point");
                    Vector3 newLocalPos = t.InverseTransformPoint(newWorldPos);
                    shape.SetPoint(i, new Vector2(newLocalPos.x, newLocalPos.y));
                    EditorUtility.SetDirty(shape);
                }
            }
        }

        // === 2. Handles de ponto médio (clicáveis pra adicionar novos vértices) ===
        if (!ctrlHeld)
        {
            Handles.color = new Color(1f, 1f, 0.4f, 0.8f); // amarelo
            for (int i = 0; i < points.Count; i++)
            {
                int next = (i + 1) % points.Count;
                Vector2 mid = (points[i] + points[next]) * 0.5f;
                Vector3 worldMid = t.TransformPoint(new Vector3(mid.x, mid.y, 0f));

                float midSize = HandleUtility.GetHandleSize(worldMid) * MIDPOINT_HANDLE_SIZE;

                if (Handles.Button(worldMid, Quaternion.identity, midSize, midSize, Handles.SphereHandleCap))
                {
                    Undo.RecordObject(shape, "Add Resin Point");
                    shape.AddPoint(mid, i);
                    EditorUtility.SetDirty(shape);
                    return;
                }
            }
        }

        // === 3. Linhas conectando os pontos (visualização do contorno) ===
        Handles.color = Color.white;
        for (int i = 0; i < points.Count; i++)
        {
            int next = (i + 1) % points.Count;
            Vector3 a = t.TransformPoint(new Vector3(points[i].x, points[i].y, 0f));
            Vector3 b = t.TransformPoint(new Vector3(points[next].x, points[next].y, 0f));
            Handles.DrawLine(a, b);
        }

        // === 4. Texto de instruções no canto da Scene View ===
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 280, 90), GUI.skin.box);
        GUILayout.Label("<b>Edição da Resina Tóxica</b>", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.Label("• Arraste os pontos azuis para mover");
        GUILayout.Label("• Clique nos pontos amarelos para inserir");
        GUILayout.Label("• Segure Ctrl e clique para deletar");
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Edite a forma do polígono diretamente na Scene View.\n" +
            "Pontos azuis: arraste para mover.\n" +
            "Pontos amarelos: clique para adicionar.\n" +
            "Ctrl+clique nos azuis: deletar (mín. 3 pontos).",
            MessageType.Info);

        ToxicResinShape shape = (ToxicResinShape)target;
        if (GUILayout.Button("Forçar Reconstrução"))
        {
            shape.Rebuild();
        }
    }
}