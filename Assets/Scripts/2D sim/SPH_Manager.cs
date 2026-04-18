using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPH_Manager : MonoBehaviour
{
    public int population;
    public float range;
    public Vector3 BoxMin;
    public Vector3 BoxMax;

    public Material material;

    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer velocitiesBuffer;
    private ComputeBuffer positionsBuffer;
    public ComputeShader computeShader;
    

    public Mesh mesh;
    private Bounds bounds;

    // Mesh Properties struct to be read from the GPU.
    // Size() is a convenience funciton which returns the stride of the struct.
    private struct MeshProperties
    {
        public Matrix4x4 mat;
        public Vector4 color;

        public static int Size()
        {
            return
                sizeof(float) * 4 * 4 + // matrix;
                sizeof(float) * 4;      // color;
        }
    }

    private void Setup()
    {
        

        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        int kernel = computeShader.FindKernel("CSMain");
        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)population;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        // Initialize buffer with the given population.
        MeshProperties[] properties = new MeshProperties[population];
        Vector4[] velocities = new Vector4[population];
        Vector3[] positions = new Vector3[population];


        for (int i = 0; i < population; i++)
        {
            MeshProperties props = new MeshProperties();
            Vector3 position = new Vector3(Random.Range(-range, range), Random.Range(-range, range),0);
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one * 0.1f;

            props.mat = Matrix4x4.TRS(position, rotation, scale);
            props.color = Color.Lerp(Color.red, Color.blue, Random.value);

            properties[i] = props;
            velocities[i] = new Vector4(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f, 0);
            positions[i] = position;
        }

        meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
        meshPropertiesBuffer.SetData(properties);
        material.SetBuffer("_Properties", meshPropertiesBuffer);

        velocitiesBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        velocitiesBuffer.SetData(velocities);
        computeShader.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
        computeShader.SetBuffer(kernel, "_Velocities", velocitiesBuffer);
        // Positions buffer (XYZ per particle)
        positionsBuffer = new ComputeBuffer(population, sizeof(float) * 3);
        positionsBuffer.SetData(positions);
        computeShader.SetBuffer(kernel, "_Positions", positionsBuffer);
        material.SetBuffer("_Positions", positionsBuffer);
        computeShader.SetVector("_BoxMin", BoxMin);
        computeShader.SetVector("_BoxMax", BoxMax);

    }


    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        int kernel = computeShader.FindKernel("CSMain");
        //computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.Dispatch(kernel, population / 64 + 1, 1, 1);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    private void OnDisable()
    {
        // Release gracefully.
        if (meshPropertiesBuffer != null)
        {
            meshPropertiesBuffer.Release();
        }
        meshPropertiesBuffer = null;

        if (argsBuffer != null)
        {
            argsBuffer.Release();
        }
        argsBuffer = null;
        if (velocitiesBuffer != null)
        {
            velocitiesBuffer.Release();
        }
        velocitiesBuffer = null;
        if (positionsBuffer != null)
        {
            positionsBuffer.Release();
        }
        positionsBuffer = null;

    }
}
