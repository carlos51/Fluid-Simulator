using UnityEngine;

public class ParticleRendererFullQuad : MonoBehaviour
{
    public Material material;
    public ParticleManager simulation;
    public float radius = 0.05f;
    ComputeBuffer buffer;
    public float worldWidth;
    public float worldHeight;

    void Start()
    {
        if (simulation == null || simulation.particleBuffer == null || simulation.particleBuffer.Length == 0) return;

        buffer = new ComputeBuffer(simulation.particleBuffer.Length, sizeof(float) * 2);
        Vector2[] ndcPositions = new Vector2[simulation.particleBuffer.Length];
        for (int i = 0; i < simulation.particleBuffer.Length; i++)
        {
            Vector2 p = simulation.particleBuffer[i];
            ndcPositions[i] = new Vector2(
                (p.x / worldWidth) * 2f - 1f,
                (p.y / worldHeight) * 2f - 1f
            );
        }
        buffer.SetData(ndcPositions);

        material.SetBuffer("_Positions", buffer);
        material.SetInt("_ParticleCount", simulation.particleBuffer.Length);
        material.SetFloat("_Radius", radius);
    }

    void Update()
    {
        if (simulation == null || simulation.particleBuffer == null || buffer == null) return;
        buffer.SetData(simulation.particleBuffer);
    }

    void OnRenderObject()
    {
        if (buffer == null) return;
        material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6);
    }

    void OnDestroy()
    {
        buffer?.Release();
    }
}
