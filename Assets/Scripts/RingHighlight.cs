using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RingHighlight : MonoBehaviour
{
    [Range(0.0f, 100.0f)]
    public float radius = 10;
    [Range(0.0f, 100.0f)]
    public float softenEdge;
    [Range(0.0f, 1.0f)]
    public float shade;
    public Transform trackedObject;

    [SerializeField] ComputeShader shader = null;

    bool firstPass = true;
    Vector2Int texSize = new Vector2Int(0, 0);
    Vector2Int groupSize = new Vector2Int();
    Camera thisCamera;
    int kernelID;
    RenderTexture output = null;
    RenderTexture renderedSource = null;
    bool init = false;
    MazeGeneratorManager _manager;
    Vector4 center;

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

        kernelID = shader.FindKernel("Highlight");

        CreateTextures();

        SetProperties();
    }

    private void OnValidate()
    {
        if (!init) Init();
        SetProperties();
    }

    void CreateTextures()
    {
        texSize.x = thisCamera.pixelWidth;
        texSize.y = thisCamera.pixelHeight;

        if (shader)
        {
            uint x, y;

            shader.GetKernelThreadGroupSizes(kernelID, out x, out y, out _);
            groupSize.x = Mathf.CeilToInt((float)texSize.x / (float)x);
            groupSize.y = Mathf.CeilToInt((float)texSize.y / (float)y);
        }
        CreateTexture(ref renderedSource);

        shader.SetTexture(kernelID, "source", renderedSource);

        CreateTexture(ref output);

        shader.SetTexture(kernelID, "output", output);
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

    void SetProperties()
    {
        float rad = (radius / 100.0f) * texSize.y;
        shader.SetFloat("radius", rad);
        shader.SetFloat("edgeWidth", rad * softenEdge / 100.0f);
        shader.SetFloat("shade", shade);
    }

    void CheckMazeReady()
    {
        _manager = MazeGeneratorManager.Instance;
        if (_manager != null && _manager.IsReady)
        {
            trackedObject = GameObject.FindWithTag("Player").transform;
            init = true;
        }
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
        shader.Dispatch(kernelID, groupSize.x, groupSize.y, 1);

        Graphics.Blit(output, renderedSource);
        Graphics.Blit(output, destination);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!init || shader == null)
        {
            Graphics.Blit(source, destination);
            CheckMazeReady();
        }
        else
        {
            if(trackedObject && thisCamera)
            {
                Vector3 pos = thisCamera.WorldToScreenPoint(trackedObject.position);
                center.x = pos.x;
                center.y = pos.y;
                shader.SetVector("center", center); 
            }
            CheckResolution(out bool resChange);
            if (resChange) SetProperties();

            if(firstPass)
                Graphics.Blit(source, renderedSource);

            DispatchWithSource(ref source, ref destination);

            firstPass = false;
        }
    }

}