using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sim2D : MonoBehaviour
{
    public int population = 1000;
    public float range = 10f;
    public Vector3 BoxMin = new Vector3(-10, -10, 0);
    public Vector3 BoxMax = new Vector3(10, 10, 0);

    public ComputeShader computeShader;
    public Mesh Mesh { get; set; }
    public ComputeBuffer ArgsBuffer { get; private set; }
    public Bounds Bounds { get; private set; }

    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer velocitiesBuffer;
    private ComputeBuffer positionsBuffer;

    private Material simMaterial;

    private struct MeshProperties
    {
        public Matrix4x4 mat;
        public Vector4 color;

        public static int Size()
        {
            return sizeof(float) * 4 * 4 + sizeof(float) * 4;
        }
    }

    public void Initialize(Material material)
    {
        simMaterial = material;

        // If Mesh is not assigned, try to get from material or elsewhere (caller should assign)
        Bounds = new Bounds(transform.position, Vector3.one * (range + 1));

        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        if (computeShader == null || Mesh == null || simMaterial == null) return;

        int kernel = computeShader.FindKernel("CSMain");

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)Mesh.GetIndexCount(0);
        args[1] = (uint)population;
        args[2] = (uint)Mesh.GetIndexStart(0);
        args[3] = (uint)Mesh.GetBaseVertex(0);
        ArgsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        ArgsBuffer.SetData(args);

        MeshProperties[] properties = new MeshProperties[population];
        Vector4[] velocities = new Vector4[population];
        Vector3[] positions = new Vector3[population];

        for (int i = 0; i < population; i++)
        {
            MeshProperties props = new MeshProperties();
            Vector3 position = new Vector3(Random.Range(-range, range), Random.Range(-range, range), 0);
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
        simMaterial.SetBuffer("_Properties", meshPropertiesBuffer);

        velocitiesBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        velocitiesBuffer.SetData(velocities);
        computeShader.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
        computeShader.SetBuffer(kernel, "_Velocities", velocitiesBuffer);

        positionsBuffer = new ComputeBuffer(population, sizeof(float) * 3);
        positionsBuffer.SetData(positions);
        computeShader.SetBuffer(kernel, "_Positions", positionsBuffer);
        simMaterial.SetBuffer("_Positions", positionsBuffer);

        computeShader.SetVector("_BoxMin", BoxMin);
        computeShader.SetVector("_BoxMax", BoxMax);
    }

    public void Simulate(float deltaTime)
    {
        if (computeShader == null) return;
        int kernel = computeShader.FindKernel("CSMain");
        // computeShader.SetFloat("_DeltaTime", deltaTime);
        computeShader.Dispatch(kernel, population / 64 + 1, 1, 1);
    }

    private void OnDisable()
    {
        if (meshPropertiesBuffer != null) meshPropertiesBuffer.Release();
        meshPropertiesBuffer = null;
        if (ArgsBuffer != null) ArgsBuffer.Release();
        ArgsBuffer = null;
        if (velocitiesBuffer != null) velocitiesBuffer.Release();
        velocitiesBuffer = null;
        if (positionsBuffer != null) positionsBuffer.Release();
        positionsBuffer = null;
    }
}
