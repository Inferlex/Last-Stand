using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour, IPointerClickHandler
{
    private Camera miniMapCam;

    public void Start()
    {
        miniMapCam = FindObjectsOfType<Camera>()[1];
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 cursor = new Vector2(0, 0);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RawImage>().rectTransform,
            eventData.pressPosition, eventData.pressEventCamera, out cursor))
        {

            Texture texture = GetComponent<RawImage>().texture;
            Rect rect = GetComponent<RawImage>().rectTransform.rect;

            float coordX = Mathf.Clamp(0, (((cursor.x - rect.x) * texture.width) / rect.width), texture.width);
            float coordY = Mathf.Clamp(0, (((cursor.y - rect.y) * texture.height) / rect.height), texture.height);

            float calX = coordX / texture.width;
            float calY = coordY / texture.height;


            cursor = new Vector2(calX, calY);

            if (eventData.button == PointerEventData.InputButton.Left)
                CastRayToWorld(cursor, false);
            else if(eventData.button == PointerEventData.InputButton.Right)
                CastRayToWorld(cursor, true);

        }

    }

    private void CastRayToWorld(Vector2 vec, bool rightClick)
    {
        Ray MapRay = miniMapCam.ScreenPointToRay(new Vector2(vec.x * miniMapCam.pixelWidth,
            vec.y * miniMapCam.pixelHeight));

        if (!rightClick) //left click
        {
            Camera.main.transform.position = new Vector3(MapRay.origin.x, MapRay.origin.y, -10f);
        }
        else
            GameEvents.current.MinimapClick(MapRay.origin);
    }
}
