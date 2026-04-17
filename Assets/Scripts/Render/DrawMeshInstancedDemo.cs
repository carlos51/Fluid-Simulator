using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMeshInstancedDemo : MonoBehaviour
{
    // How many meshes to draw.
    public int population;
    // Range to draw meshes within.
    public float range;

    // Material to use for drawing the meshes.
    public Material material;

    private Matrix4x4[] matrices;
    private MaterialPropertyBlock block;

    public Mesh mesh;

    private void Setup()
    {
        // 2. Eliminamos la creación manual del mesh por código
        // Si 'mesh' es nulo, lanzamos un error para avisarte en el Inspector
        if (mesh == null)
        {
            Debug.LogError("¡Falta asignar el Mesh en el Inspector!");
            return;
        }

        matrices = new Matrix4x4[population];
        Vector4[] colors = new Vector4[population];

        block = new MaterialPropertyBlock();

        for (int i = 0; i < population; i++)
        {
            // Build matrix.
            Vector3 position = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
            Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
            Vector3 scale = Vector3.one;


            matrices[i] = Matrix4x4.TRS(position, rotation, scale);

            colors[i] = Color.Lerp(Color.red, Color.blue, Random.value);
        }

        // Custom shader needed to read these!!
        block.SetVectorArray("_Colors", colors);
    }
    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        // Draw a bunch of meshes each frame.
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, population, block);
    }
}
