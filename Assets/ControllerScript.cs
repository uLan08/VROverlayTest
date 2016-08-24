using UnityEngine;
using System.Collections;

public class ControllerScript : MonoBehaviour {

    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;
    public Overlay overlay;
    bool hasOverlay = false;

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
            if (!hasOverlay)
            {
                overlay.createOverlay();
                hasOverlay = true;

            }
            else
            {
                overlay.destroyOverlay();
                hasOverlay = false;
            }
        }
    }
}
