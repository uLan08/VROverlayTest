using UnityEngine;
using System.Collections;

public class ControllerScript : MonoBehaviour {

    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;
    public Overlay overlay;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
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
            }
            else
            {
                overlay.gameObject.SetActive(false);
            }
        }
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Debug.Log("asdasd");
            Debug.Log("controller pos : " + gameObject.transform.position);
            Debug.Log("controller for : " + gameObject.transform.forward);
            //RectTransform rt = (RectTransform)overlay.transform;
            //float width = rt.rect.width;
            //float height = rt.rect.height;
            //Debug.Log("overlay width? " + width);
            //Debug.Log("overlay height? " + height);
            Debug.Log(overlay.getUVs(gameObject.transform.position, gameObject.transform.forward));
        }
    }
}
