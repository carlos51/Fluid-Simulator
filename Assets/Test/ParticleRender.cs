using UnityEngine;

public class ParticleRender : MonoBehaviour
{
    public int particleCount = 10000;
    public Shader shader;
    public Mesh mesh;
    public float particleSize = 0.05f;

    Material material;
    ComputeBuffer positionBuffer;

    Vector3[] positions;

    void Start()
    {
        // Crear material
        material = new Material(shader);

        // Inicializar posiciones (ejemplo: aleatorias en [-1,1])
        positions = new Vector3[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            positions[i] = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0f
            );
        }

        // Crear ComputeBuffer
        positionBuffer = new ComputeBuffer(particleCount, sizeof(float) * 3);
        positionBuffer.SetData(positions);

        // Enviar buffer al shader
        material.SetBuffer("_Positions", positionBuffer);
        material.SetFloat("_Size", particleSize);
    }

    void Update()
    {
        // Renderizar partículas
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material,
            new Bounds(Vector3.zero, Vector3.one * 10000f), positionBuffer);
    }

    void OnDestroy()
    {
        if (positionBuffer != null) positionBuffer.Release();
    }
}
