using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente que sincroniza pontos de um polígono editável com:
/// 1. PolygonCollider2D (para a mecânica do trigger)
/// 2. Mesh procedural com UV.y = 0 no centro e UV.y = 1 na borda (para o shader)
///
/// Os pontos são em espaço LOCAL (relativos à posição do GameObject).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ToxicResinShape : MonoBehaviour
{
    [Header("Pontos do Polígono (espaço local)")]
    [Tooltip("Pontos que formam o contorno da poça. Edite via Scene View (passo seguinte).")]
    [SerializeField]
    private List<Vector2> points = new List<Vector2>
    {
        new Vector2(-1f, -1f),
        new Vector2( 1f, -1f),
        new Vector2( 1f,  1f),
        new Vector2(-1f,  1f)
    };

    [Header("Auto-Sync")]
    [Tooltip("Quando true, recalcula collider e mesh sempre que os pontos mudam.")]
    [SerializeField] private bool autoSync = true;
    
    [Header("Deformação Orgânica")]
    [Tooltip("Quantos segmentos extras inserir entre cada par de pontos. 0 = polígono original (retas).")]
    [Range(0, 8)]
    [SerializeField] private int subdivisions = 4;

    [Tooltip("Intensidade do deslocamento orgânico nas bordas (0 = sem deformação).")]
    [Range(0f, 0.5f)]
    [SerializeField] private float deformationAmount = 0.15f;

    [Tooltip("Frequência do ruído de deformação. Mais alto = ondulações mais curtas/rápidas.")]
    [Range(0.1f, 10f)]
    [SerializeField] private float deformationFrequency = 2.5f;

    [Tooltip("Seed do ruído. Mude pra ter uma forma deformada diferente sem mexer nos pontos.")]
    [SerializeField] private int deformationSeed = 0;

    // Cache de componentes
    private PolygonCollider2D _polyCollider;
    private MeshFilter _meshFilter;
    private Mesh _generatedMesh;
    private MeshRenderer _meshRenderer;

    public List<Vector2> Points => points;

    private void OnEnable()
    {
        EnsureReferences();
        Rebuild();
    }

    private void OnValidate()
    {
        if (!autoSync) return;
        // OnValidate roda quando algo muda no Inspector
        EnsureReferences();
        Rebuild();
    }

    private void EnsureReferences()
    {
        if (_polyCollider == null) _polyCollider = GetComponent<PolygonCollider2D>();
        if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
        if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Regenera collider e mesh a partir dos pontos atuais.
    /// </summary>
    public void Rebuild()
    {
        EnsureReferences();
        if (points == null || points.Count < 3) return;

        // Gera os pontos "finais" — subdivididos e deformados — uma única vez,
        // e usa eles tanto pro collider quanto pra mesh (garantindo sincronia).
        List<Vector2> processedPoints = GenerateProcessedPoints();

        SyncCollider(processedPoints);
        SyncMesh(processedPoints);
    }

    private void SyncCollider(List<Vector2> processedPoints)
    {
        _polyCollider.isTrigger = true;
        _polyCollider.SetPath(0, processedPoints.ToArray());
    }

    private void SyncMesh(List<Vector2> processedPoints)
    {
        int n = processedPoints.Count;
        if (n < 3) return;

        // 1. Calcular centróide (apenas para referência de "distância ao centro")
        Vector2 centroid = Vector2.zero;
        foreach (var p in processedPoints) centroid += p;
        centroid /= n;

        // 2. Calcular distância máxima do centróide a qualquer ponto da borda (para normalizar UV.y)
        float maxDist = 0f;
        foreach (var p in processedPoints)
        {
            float d = Vector2.Distance(p, centroid);
            if (d > maxDist) maxDist = d;
        }
        if (maxDist < 0.0001f) return;

        // 3. Vértices: apenas os pontos da borda, com UV.y = 1 (todos são "borda")
        // O shader vai usar UV.y junto com a posição relativa para calcular o contorno.
        Vector3[] vertices = new Vector3[n];
        Vector2[] uvs = new Vector2[n];

        for (int i = 0; i < n; i++)
        {
            vertices[i] = new Vector3(processedPoints[i].x, processedPoints[i].y, 0f);
            // UV.x distribuído ao redor do contorno
            uvs[i] = new Vector2((float)i / n, 1f);
        }

        // 4. Triangulação: ear clipping cobre tanto convexos quanto côncavos uniformemente
        int[] triangles = Triangulator.Triangulate(processedPoints);

        // 5. Aplicar na mesh
        if (_generatedMesh == null)
        {
            _generatedMesh = new Mesh();
            _generatedMesh.name = "ToxicResinMesh_Generated";
        }
        _generatedMesh.Clear();
        _generatedMesh.vertices = vertices;
        _generatedMesh.uv = uvs;
        _generatedMesh.triangles = triangles;
        _generatedMesh.RecalculateNormals();
        _generatedMesh.RecalculateBounds();

        _meshFilter.sharedMesh = _generatedMesh;

        // Envia o raio máximo pro shader (por instância, sem duplicar material)
        UpdateShaderLocalRadius(maxDist);
    }

    /// <summary>
    /// Gera os pontos finais: subdivide cada aresta em N segmentos e aplica deslocamento
    /// orgânico baseado em ruído. O resultado é uma silhueta curva/viscosa.
    /// </summary>
    private List<Vector2> GenerateProcessedPoints()
    {
        List<Vector2> result = new List<Vector2>();
        int n = points.Count;

        for (int i = 0; i < n; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % n];

            // Direção normal (perpendicular à aresta), usada para deslocar pra dentro/fora
            Vector2 edge = b - a;
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized;

            // Pra cada segmento, criamos (subdivisions + 1) pontos
            int segments = subdivisions + 1;
            for (int s = 0; s < segments; s++)
            {
                float t = (float)s / segments;
                Vector2 linearPoint = Vector2.Lerp(a, b, t);

                // Calcula deslocamento via ruído.
                // O ruído tem que ser CONSISTENTE: mesmo ponto da aresta = mesmo deslocamento.
                // Usamos coordenadas mundiais (posição final do ponto) + seed.
                float noiseInput = (linearPoint.x + linearPoint.y * 1.7f) * deformationFrequency + deformationSeed * 13.7f;
                float displacement = (Mathf.PerlinNoise(noiseInput, noiseInput * 0.7f) - 0.5f) * 2f;

                // Suaviza: deslocamento é zero nas pontas (s=0) e máximo no meio (s=segments/2)
                // Isso evita "quinas" estranhas nos vértices originais.
                float taper = Mathf.Sin(t * Mathf.PI); // 0 nas pontas, 1 no meio

                Vector2 finalPoint = linearPoint + normal * displacement * deformationAmount * taper;
                result.Add(finalPoint);
            }
        }

        return result;
    }

    private static readonly int LocalRadiusID = Shader.PropertyToID("_LocalRadius");
    private MaterialPropertyBlock _propBlock;

    private void UpdateShaderLocalRadius(float radius)
    {
        if (_meshRenderer == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        _meshRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(LocalRadiusID, radius);
        _meshRenderer.SetPropertyBlock(_propBlock);
    }

    /// <summary>
    // Métodos públicos para o Editor customizado chamar
    public void SetPoint(int index, Vector2 worldOrLocalPos)
    {
        if (index < 0 || index >= points.Count) return;
        points[index] = worldOrLocalPos;
        if (autoSync) Rebuild();
    }

    public void AddPoint(Vector2 localPos, int afterIndex = -1)
    {
        if (afterIndex < 0 || afterIndex >= points.Count) points.Add(localPos);
        else points.Insert(afterIndex + 1, localPos);
        if (autoSync) Rebuild();
    }

    public void RemovePoint(int index)
    {
        if (points.Count <= 3) return; // mínimo 3 pontos
        if (index < 0 || index >= points.Count) return;
        points.RemoveAt(index);
        if (autoSync) Rebuild();
    }
}