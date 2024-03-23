using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Skidmarks : MonoBehaviour {

    public const float MARK_WIDTH = 0.25f;
    readonly Color32 zeroColor = new Color32(0, 0, 0, 0);

    private class Section {
        public Vector3 position = Vector3.zero;
        public Vector3 normal = Vector3.zero;
        public Vector4 tangent = Vector4.zero;
        public Vector3 positionLeft = Vector3.zero;
        public Vector3 positionRight = Vector3.zero;
        public byte intensity;
        public int lastIndex;
    }

    [SerializeField] Material skidmarksMaterial;
    [SerializeField] int maxSections = 1024;
    float minDistance = 0.5f;
    Section[] skidmarks;
    Mesh mesh;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    Vector3[] vertices;
    Vector3[] normals;
    Vector4[] tangents;
    Color32[] colors;
    Vector2[] uvs;

    int markIndex;
    int[] triangles;

    bool meshUpdated;
    bool boundsSet;

    void Start() {

        if (meshRenderer == null)
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (meshFilter == null)
            meshFilter = gameObject.GetComponent<MeshFilter>();

        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.material = skidmarksMaterial;
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;

        Init();

    }

    private void Init() {
        skidmarks = new Section[maxSections];
        markIndex = 0;
        boundsSet = false;

        for (int i = 0; i < maxSections; i++)
            skidmarks[i] = new Section();

        mesh = new Mesh();
        mesh.MarkDynamic();

        meshFilter.sharedMesh = mesh;
        vertices = new Vector3[maxSections * 4];
        normals = new Vector3[maxSections * 4];
        tangents = new Vector4[maxSections * 4];
        colors = new Color32[maxSections * 4];
        uvs = new Vector2[maxSections * 4];
        triangles = new int[maxSections * 6];
    }

    void LateUpdate() {
        if (!meshUpdated)
            return;

        meshUpdated = false;
        mesh.SetVertices(vertices);
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.triangles = triangles;
        mesh.colors32 = colors;
        mesh.uv = uvs;

        if (!boundsSet) {
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
            boundsSet = true;
        }
        meshFilter.sharedMesh = mesh;
    }

    void UpdateMesh() {
        Section markSection = skidmarks[markIndex];

        if (markSection.lastIndex == -1) 
            return;

        Section markSection2 = skidmarks[markSection.lastIndex];

        vertices[markIndex * 4] = markSection2.positionLeft;
        vertices[markIndex * 4 + 1] = markSection2.positionRight;
        vertices[markIndex * 4 + 2] = markSection.positionLeft;
        vertices[markIndex * 4 + 3] = markSection.positionRight;

        normals[markIndex * 4] = markSection2.normal;
        normals[markIndex * 4 + 1] = markSection2.normal;
        normals[markIndex * 4 + 2] = markSection.normal;
        normals[markIndex * 4 + 3] = markSection.normal;

        tangents[markIndex * 4] = markSection2.tangent;
        tangents[markIndex * 4 + 1] = markSection2.tangent;
        tangents[markIndex * 4 + 2] = markSection.tangent;
        tangents[markIndex * 4 + 3] = markSection.tangent;

        colors[markIndex * 4] = new Color32(0, 0, 0, markSection2.intensity);
        colors[markIndex * 4 + 1] = new Color32(0, 0, 0, markSection2.intensity);
        colors[markIndex * 4 + 2] = new Color32(0, 0, 0, markSection.intensity);
        colors[markIndex * 4 + 3] = new Color32(0, 0, 0, markSection.intensity);

        uvs[markIndex * 4] = new Vector2(0f, 0f);
        uvs[markIndex * 4 + 1] = new Vector2(1f, 0f);
        uvs[markIndex * 4 + 2] = new Vector2(0f, 1f);
        uvs[markIndex * 4 + 3] = new Vector2(1f, 1f);

        triangles[markIndex * 6] = markIndex * 4;
        triangles[markIndex * 6 + 2] = markIndex * 4 + 1;
        triangles[markIndex * 6 + 1] = markIndex * 4 + 2;
        triangles[markIndex * 6 + 3] = markIndex * 4 + 2;
        triangles[markIndex * 6 + 5] = markIndex * 4 + 1;
        triangles[markIndex * 6 + 4] = markIndex * 4 + 3;

        meshUpdated = true;
    }

    public void ClearAll() {
        Destroy(mesh);
        Init();

        for (int i = 0; i < maxSections; i++) {
            colors[i * 4] = zeroColor;
            colors[i * 4 + 1] = zeroColor;
            colors[i * 4 + 2] = zeroColor;
            colors[i * 4 + 3] = zeroColor;
        }

        UpdateMesh();
    }

    public int Add(Vector3 pos, Vector3 normal, float intensity, int lastIndex) {
        if (intensity > 1f) 
            intensity = 1f;
        else if (intensity < 0f)
            return -1;

        intensity = Mathf.Clamp(intensity, 0f, 0.6f);

        if (lastIndex > 0 && (pos - skidmarks[lastIndex].position).sqrMagnitude < minDistance * minDistance)
            return lastIndex;

        Section markSection = skidmarks[markIndex];
        markSection.position = pos + normal * 0.02f;
        markSection.normal = normal;
        markSection.intensity = (byte)(intensity * 255f);
        markSection.lastIndex = lastIndex;

        if (lastIndex != -1) {
            Section markSection2 = skidmarks[lastIndex];
            Vector3 normalized = Vector3.Cross(markSection.position - markSection2.position, normal).normalized;

            markSection.positionLeft = markSection.position + normalized * MARK_WIDTH * 0.5f;
            markSection.positionRight = markSection.position - normalized * MARK_WIDTH * 0.5f;
            markSection.tangent = new Vector4(normalized.x, normalized.y, normalized.z, 1f);

            if (markSection2.lastIndex == -1) {
                markSection2.tangent = markSection.tangent;
                markSection2.positionLeft = markSection.position + normalized * MARK_WIDTH * 0.5f;
                markSection2.positionRight = markSection.position - normalized * MARK_WIDTH * 0.5f;
            }
        }

        UpdateMesh();

        int result = markIndex;
        int num = markIndex + 1;
        markIndex = num;
        markIndex = num % maxSections;
        return result;
    }
}
