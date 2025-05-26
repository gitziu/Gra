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
    public float cameraMargin = 50f;
    public float gameAreaMinX = 0f;
    public float gameAreaMinY = 0f;
    public float gameAreaMaxX = 1000f; 
    public float gameAreaMaxY = 1000f;
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
        if (!LevelEditorManager.Instance.cameraDrag) return;
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragOrigin;
            diff = new Vector3(-diff.x, -diff.y, 0);
            transform.Translate(diff);
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -100, 100), transform.position.y, transform.position.z);
            ClampCameraPosition();
            Debug.Log(transform.position);
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }
    
        void ClampCameraPosition()
    {
        // Obliczamy połowę wysokości i szerokości widoku kamery w jednostkach świata
        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        // Obliczamy minimalne i maksymalne pozycje X i Y dla centrum kamery
        // Bierzemy pod uwagę granice obszaru gry i rozmiar kamery
        float minCamX = gameAreaMinX + camHalfWidth - cameraMargin;
        float maxCamX = gameAreaMaxX - camHalfWidth + cameraMargin;
        float minCamY = gameAreaMinY + camHalfHeight - cameraMargin;
        float maxCamY = gameAreaMaxY - camHalfHeight + cameraMargin;

        // Pobieramy aktualną pozycję kamery
        Vector3 currentCamPos = transform.position;

        // Ograniczamy pozycję kamery
        float clampedX = Mathf.Clamp(currentCamPos.x, minCamX, maxCamX);
        float clampedY = Mathf.Clamp(currentCamPos.y, minCamY, maxCamY);

        // Ustawiamy nową, ograniczoną pozycję
        transform.position = new Vector3(clampedX, clampedY, currentCamPos.z);
    }

}