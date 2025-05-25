using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraDrag : MonoBehaviour
{
    private Camera cam;
    public float speed = 10.5f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    private float velocity = 0f;
    public float zoomTime = 0.25f;
    private float zoom;
    public float zoomSpeed = 10f;
    private Vector3 dragOrigin;


    void Awake()
    {
        cam = Camera.main;
        zoom = cam.orthographicSize;
    }
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoom -= scroll * zoomSpeed;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, zoom, ref velocity, zoomTime);
        if(!LevelEditorManager.Instance.cameraDrag) return;
        if(Input.GetMouseButtonDown(0)) {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if(Input.GetMouseButton(0)){
            Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragOrigin;
            diff = new Vector3(-diff.x, -diff.y, 0);
            transform.Translate(diff);
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -100, 100), transform.position.y, transform.position.z);
            Debug.Log(transform.position);
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}