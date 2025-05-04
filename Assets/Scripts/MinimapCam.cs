using UnityEngine;

public class AssignRenderTexture : MonoBehaviour
{
    public Camera minimapCamera;
    public RenderTexture renderTexture;
    public Canvas canvas;

    void Start()
    {
        minimapCamera.targetTexture = renderTexture;
    }
}