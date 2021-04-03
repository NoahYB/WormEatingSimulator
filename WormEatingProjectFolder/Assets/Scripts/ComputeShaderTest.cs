using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    public ComputeShader computeTexture;

    public ComputeShader computeParticle;

    public RenderTexture renderTexture;

    public int resolution;

    public int n;

    ComputeBuffer particleBuffer;

    ComputeBuffer particleBoardBuffer;

    int[] particleBoard;

    Particle[] particleArray;

    private int i;

    struct Particle
    {
        public Vector2 position;
        public Vector2 velocity;
        public float t;
    }
    private void Update()
    {
        computeParticle.Dispatch(0, Mathf.CeilToInt(n/128), 1, 1);

        i++;

        if (Input.GetMouseButton(0))
        {
            computeParticle.SetFloat("targetx", Input.mousePosition.x);
            computeParticle.SetFloat("targety", Input.mousePosition.y);
        }
        else
        {
            computeParticle.SetFloat("targetx",0.0f);
            computeParticle.SetFloat("targety", 0.0f);
        }
    }
    private void Start()
    {

        //Set texture if texture = null
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(resolution, resolution, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
        //Create buffers for texture array and particle array
        CreateBuffers();
        computeParticle.SetBuffer(0, "particleArray", particleBoardBuffer);
        computeParticle.SetBuffer(0, "particles", particleBuffer);
        //Compute particles set variables
        computeParticle.SetInt("n", n);
        computeParticle.SetFloat("resolution", resolution);
        computeParticle.SetFloat("targetx", 0.0f);
        computeParticle.SetFloat("targety", 0.0f);


        //pass buffer to shader
        computeTexture.SetFloat("resolution", resolution);
        computeTexture.SetFloat("n", n);
        computeTexture.SetBuffer(0, "particleArray", particleBoardBuffer);

        
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Set data in compute shader and dispatch shader each draw      
        computeTexture.SetTexture(0, "Result", renderTexture);
        computeTexture.Dispatch(0, renderTexture.width, renderTexture.height, 1);
        computeTexture.SetBuffer(0, "particlePositions", particleBuffer);
        //Blit the texture to the screen
        Graphics.Blit(renderTexture, destination);
        
    }
    private void CreateBuffers()
    {
        //Create the particle buffer
        particleArray = CreateParticles();
        int totalSize = sizeof(float) * 2  + sizeof(float) * 2 + sizeof(float); 
        particleBuffer = new ComputeBuffer
            (particleArray.Length, totalSize);
        particleBuffer.SetData(particleArray);

        //Create the array to hold positions buffer
        particleBoard = new int[resolution * resolution];
        particleBoardBuffer = new ComputeBuffer
            (particleBoard.Length, 4);
        particleBoardBuffer.SetData(particleBoard);
    }
    private Particle[] CreateParticles()
    {
        Particle[] particlePositions = new Particle[n];
        for (int i = 0; i < n; i++)
        {
            particlePositions[i].position = new Vector2(resolution / 2 + Random.Range(-1,1), resolution/ 2 + Random.Range(-1, 1));
            particlePositions[i].velocity =
                new Vector2(Random.Range(-.002f, .002f) *resolution, Random.Range(-.002f, .002f) * resolution);
            particlePositions[i].t = 10;
        }
        return particlePositions;
    }

}
