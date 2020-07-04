using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdsEyeFollow : MonoBehaviour
{
    private GameObject target;
    private float zoom = 5.5f;
    private float minZoom = 0.5f;
    private float maxZoom =10.0f;
    private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Bot");
        camera = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            transform.position = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        }

        zoom = Mathf.Clamp(zoom - Input.GetAxis("Mouse ScrollWheel") * 5, minZoom, maxZoom);
        camera.orthographicSize = zoom;
    }
}
