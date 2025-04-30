using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraDrag : MonoBehaviour
{
    public LevelEditorManager man;
    private Camera cam;
    public float speed = 10.5f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    private float velocity = 0f;
    public float zoomTime = 0.25f;
    private float zoom;
    public float zoomSpeed = 4f;
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
        if(!man.Instance.cameraDrag) return;
        if(Input.GetMouseButtonDown(0)) {
            //dragOrigin = Input.mousePosition;
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if(Input.GetMouseButton(0)){
            //Vector3 diff = Input.mousePosition - dragOrigin;
            Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragOrigin;
            diff = new Vector3(-diff.x, -diff.y, 0);// * speed * Time.deltaTime;
            transform.Translate(diff);
            /*
            Vector3 diff = Input.mousePosition - dragOrigin;
            Vector3 move = new Vector3(-diff.x, -diff.y, z_position);
            Vector3 target = transform.position + move;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            */
        }
        //dragOrigin = Input.mousePosition;
    }
}