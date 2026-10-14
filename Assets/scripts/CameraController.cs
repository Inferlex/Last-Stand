using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float panSpeed = 30f;
    private float panBorderThickness = 10f;
    private float scale = 7;
    private float defaultScale = 7;
    public int maxSize;
    public int minSize;
    private GameObject[] borders;

    void Start()
    {
        Border bord = FindObjectOfType<Border>();
        borders = bord.Borders;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
       
        CameraMovement(ref pos);

       if (borders[0].GetComponent<Renderer>().isVisible)
        {
            pos.x += 0.3f;
        }
        else if (borders[1].GetComponent<Renderer>().isVisible)
        {
            pos.x -= 0.3f;
        }
        else if (borders[2].GetComponent<Renderer>().isVisible)
        {
            pos.y -= 0.3f;
        }
        else if (borders[3].GetComponent<Renderer>().isVisible)
        {
            pos.y += 0.3f;
        }

        CameraZoom();
        gameObject.GetComponent<Camera>().orthographicSize = scale;
        transform.position = pos;
        float newScale = scale - defaultScale;
        newScale *= 0.14f;
        gameObject.transform.localScale = new Vector3(1f + newScale, 1f + newScale, 1);
        
    }

    void CameraMovement(ref Vector3 pos)
    {
        if ((Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - panBorderThickness) && !borders[2].GetComponent<Renderer>().isVisible)
        {
            pos.y += panSpeed * Time.deltaTime;

        }
        if ((Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= panBorderThickness) && !borders[0].GetComponent<Renderer>().isVisible)
        {
            pos.x -= panSpeed * Time.deltaTime;

        }
        if ((Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= panBorderThickness) && !borders[3].GetComponent<Renderer>().isVisible)
        {
            pos.y -= panSpeed * Time.deltaTime;

        }
        if ((Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - panBorderThickness) && !borders[1].GetComponent<Renderer>().isVisible)
        {
            pos.x += panSpeed * Time.deltaTime;

        }
    }

    void CameraZoom()
    {
        scale -= Input.mouseScrollDelta.y;
        if(scale > maxSize)
        {
            scale = maxSize;
        }
        else if (scale < minSize)
        {
            scale = minSize;
        }
    }

    public void ChangePosition(Vector2 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, -10);
    }
}
