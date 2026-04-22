using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPH_Manager : MonoBehaviour
{
    public int population;
    public float particleSize;
    public float range;
    public Vector3 BoxMin;
    public Vector3 BoxMax;
    public float H; // kernel radius
    public float k; // stiffness constant
    public float targetDensity;
    public float G; // gravity constant
    public Material material;



    
    private ComputeBuffer argsBuffer;
    private ComputeBuffer velocitiesBuffer;
    private ComputeBuffer positionsBuffer;
    public ComputeShader computeShader;
    private ComputeBuffer colorsBuffer;
    private ComputeBuffer forcesBuffer;
    private ComputeBuffer densitiesBuffer;
    

    public Mesh mesh;
    private Bounds bounds;

    // MeshProperties removed — no longer used for per-instance data from CPU.

    private void Setup()
    {
        

        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        int kernel = computeShader.FindKernel("Integrate");
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

        // Initialize buffers for the given population.
        Vector4[] velocities = new Vector4[population];
        Vector4[] positions = new Vector4[population];
        Vector4[] forces = new Vector4[population];

        float[] densities = new float[population];
        Vector4[] colors = new Vector4[population];


        // Arrange particles in a centered 2D grid covering [-range, range]
        int cols = Mathf.CeilToInt(Mathf.Sqrt(population));
        int rows = Mathf.CeilToInt((float)population / cols);
        float spacingX = (cols > 1) ? (2f * range) / (cols) : 0f;
        float spacingY = (rows > 1) ? (2f * range) / (rows) : 0f;

        for (int i = 0; i < population; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float x = -range + (col + 0.5f) * spacingX;
            float y = -range + (row + 0.5f) * spacingY;
            positions[i] = new Vector4(x, y, 0f, 0f);
            // start stationary
            velocities[i] = Vector4.zero;
            colors[i] = Color.Lerp(Color.red, Color.blue, Random.value);
            forces[i] = Vector4.zero;
        }


        colorsBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        colorsBuffer.SetData(colors);
        material.SetBuffer("_Colors", colorsBuffer);

        forcesBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        forcesBuffer.SetData(forces);
        computeShader.SetBuffer(kernel, "_Forces", forcesBuffer);

        velocitiesBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        velocitiesBuffer.SetData(velocities);
        computeShader.SetBuffer(kernel, "_Velocities", velocitiesBuffer);

        positionsBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        positionsBuffer.SetData(positions);
        computeShader.SetBuffer(kernel, "_Positions", positionsBuffer);

        densitiesBuffer = new ComputeBuffer(population, sizeof(float));
        computeShader.SetBuffer(kernel, "_Densities", densitiesBuffer);

        material.SetBuffer("_Positions", positionsBuffer);
        material.SetFloat("_ParticleSize", particleSize);
        material.SetBuffer("_Colors", colorsBuffer);
        material.SetFloat("_Size", particleSize);

        computeShader.SetVector("_BoxMin", BoxMin);
        computeShader.SetVector("_BoxMax", BoxMax);
        computeShader.SetInt("_NumParticles", population);
        computeShader.SetFloat("_H", H);
        computeShader.SetFloat("_K", k);
        computeShader.SetFloat("_TargetDensity", targetDensity);
        computeShader.SetFloat("_G", G);





    }


    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        int integrate = computeShader.FindKernel("Integrate");
        //computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.Dispatch(integrate, population / 64 + 1, 1, 1);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    private void OnDisable()
    {
        // Release gracefully.

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
        if (colorsBuffer != null)
        {
            colorsBuffer.Release();
        }
        colorsBuffer = null;
        if (forcesBuffer != null)
        {
            forcesBuffer.Release();
        }
        forcesBuffer = null;
        if (densitiesBuffer != null)
        {
            densitiesBuffer.Release();
        }
        densitiesBuffer = null;

    }


}
