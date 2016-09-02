using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControllerScript : MonoBehaviour {

    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;
    public Overlay overlay;
    public GameObject canvas;
    public Sprite cursorSprite;
    GameObject cursor;
    bool hasOverlay = false;
    float x;
    float y;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start()
    {
        cursor = new GameObject("cursor");
       
        Image image = cursor.AddComponent<Image>();
        image.sprite = cursorSprite;

        cursor.transform.SetParent(canvas.transform);
        cursor.transform.localPosition = Vector3.zero;
        cursor.transform.localRotation = Quaternion.identity;
        cursor.transform.localScale = Vector3.one / 2;


    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.System))
        {
            Debug.Log("pressed system button");
            if (!overlay.gameObject.activeSelf)
            {
                overlay.gameObject.SetActive(true);
                hasOverlay = true;
            }
            else
            {
                overlay.gameObject.SetActive(false);
                hasOverlay = false;
            }
        }
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            //Debug.Log("asdasd");
            Debug.Log("controller pos : " + gameObject.transform.position);
            Debug.Log("controller for : " + gameObject.transform.forward);

            //RectTransform rt = (RectTransform)overlay.transform;
            //float width = rt.rect.width;
            //float height = rt.rect.height;
            //Debug.Log("overlay width? " + width);
            //Debug.Log("overlay height? " + height);

            // cursor.transform.position = pos;

        }
        if (hasOverlay)
        {
            var uvs = overlay.getUVs(gameObject.transform.position, gameObject.transform.forward);
            RectTransform rt = (RectTransform)canvas.transform;
            x = uvs.x * 400f;
            y = uvs.y * 256f;

            Debug.Log("this is x " + x);
            Debug.Log("this is y " + y);
            if( x <= 400 && y <= 256)
            {
                x -= 200;
                y -= 128;
            }
            Vector2 pos = new Vector2(x, y);
            cursor.transform.localPosition = pos;
        }
    }
}
