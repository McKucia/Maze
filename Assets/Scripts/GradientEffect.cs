using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GradientEffect : MonoBehaviour
{
    [SerializeField] ComputeShader shader = null;

    Vector2Int texSize = new Vector2Int(0, 0);
    Vector2Int groupSize = new Vector2Int();
    Camera thisCamera;

    int kernelGaussID;
    int kernelSobelID;

    RenderTexture sobelOutput = null;
    RenderTexture gaussOutput = null;
    RenderTexture renderedSource = null;

    ComputeBuffer gaussWeightsBuffer;
    float[] gaussWeights;
    float sigma = 2.0f;
    bool init = false;

    void Start()
    {
        Init();
    }

    void Init()
    {

        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("It seems your target Hardware does not support Compute Shaders.");
            return;
        }

        if (!shader)
        {
            Debug.LogError("No shader");
            return;
        }

        thisCamera = GetComponent<Camera>();

        if (!thisCamera)
        {
            Debug.LogError("Object has no Camera");
            return;
        }

        kernelGaussID = shader.FindKernel("GaussBlur");
        kernelSobelID = shader.FindKernel("Sobel");

        CalculateWeights();
        InitGPUBuffers();
        CreateTextures();

        init = true;
    }

    void InitGPUBuffers()
    {
        gaussWeightsBuffer = new ComputeBuffer(gaussWeights.Length, sizeof(float));
        gaussWeightsBuffer.SetData(gaussWeights);

        shader.SetBuffer(kernelGaussID, "gaussWeightsBuffer", gaussWeightsBuffer);
    }

    void CalculateWeights()
    {
        int counter = 0;
        int halfWidth = (int)(2 * sigma);
        gaussWeights = new float[(halfWidth * 2 + 1) * (halfWidth * 2 + 1)];

        for (int x = -halfWidth; x <= halfWidth; x++)
            for (int y = -halfWidth; y <= halfWidth; y++)
                gaussWeights[counter++] += GetGaussWeight(sigma, x, y);
    }

    float GetGaussWeight(float sigma, float posX, float posY)
    {
        return (1.0f / Mathf.Sqrt(2.0f * Mathf.PI * sigma * sigma)) * Mathf.Exp(-(posX * posX + posY * posY) / (2.0f * sigma * sigma));
    }

    void CreateTextures()
    {
        texSize.x = thisCamera.pixelWidth;
        texSize.y = thisCamera.pixelHeight;

        if (shader)
        {
            uint x, y;

            shader.GetKernelThreadGroupSizes(kernelSobelID, out x, out y, out _);
            groupSize.x = Mathf.CeilToInt((float)texSize.x / (float)x);
            groupSize.y = Mathf.CeilToInt((float)texSize.y / (float)y);
        }
        CreateTexture(ref renderedSource);

        shader.SetTexture(kernelSobelID, "source", renderedSource);

        CreateTexture(ref sobelOutput);
        CreateTexture(ref gaussOutput);

        shader.SetTexture(kernelSobelID, "sobelOutput", sobelOutput);
        shader.SetTexture(kernelGaussID, "sobelOutput", sobelOutput);
        shader.SetTexture(kernelGaussID, "gaussOutput", gaussOutput);
    }

    void ClearTextures()
    {
        ClearTexture(ref renderedSource);
    }

    void ClearTexture(ref RenderTexture textureToClear)
    {
        if (null != textureToClear)
        {
            textureToClear.Release();
            textureToClear = null;
        }
    }

    void CreateTexture(ref RenderTexture textureToMake, int divide = 1)
    {
        textureToMake = new RenderTexture(texSize.x / divide, texSize.y / divide, 0);
        textureToMake.enableRandomWrite = true;
        textureToMake.Create();
    }

    void CheckResolution(out bool resChange)
    {
        resChange = false;

        if (texSize.x != thisCamera.pixelWidth || texSize.y != thisCamera.pixelHeight)
        {
            resChange = true;
            CreateTextures();
        }
    }

    void DispatchWithSource(ref RenderTexture source, ref RenderTexture destination)
    {
        if (!init) return;

        Graphics.Blit(source, renderedSource);

        shader.Dispatch(kernelSobelID, groupSize.x, groupSize.y, 1);
        //shader.Dispatch(kernelGaussID, groupSize.x, groupSize.y, 1);

        Graphics.Blit(sobelOutput, destination);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (shader == null)
        {
            Graphics.Blit(source, destination);
        }
        else
        {
            bool resChange = false;
            CheckResolution(out resChange);
            DispatchWithSource(ref source, ref destination);
        }
    }

    private void OnDestroy()
    {
        gaussWeightsBuffer.Dispose();
    }
}