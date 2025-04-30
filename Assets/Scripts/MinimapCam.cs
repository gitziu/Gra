using UnityEngine;

public class AssignRenderTexture : MonoBehaviour
{
    public Camera minimapCamera;
    public RenderTexture renderTexture;

    void Start()
    {
        minimapCamera.targetTexture = renderTexture;
    }
}