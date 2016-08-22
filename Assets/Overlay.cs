using UnityEngine;
using System.Collections;
using Valve.VR;

public class Overlay : MonoBehaviour {

    public Texture overlayTexture;

    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;
    bool hasOverlay = false;
    ulong overlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

	void Start ()
    {
	
	}
	
	void Update ()
    {
        device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.System))
        {
            Debug.Log("pressed system button");
            if (!hasOverlay)
            {
                createOverlay();
                hasOverlay = true;

            }
            else
            {
                destroyOverlay();
                hasOverlay = false;
            }
        }
	}

    void createOverlay()
    {
        var steamvr = SteamVR.instance;
        var overlay = OpenVR.Overlay;
        if (overlay == null) return;
        var error = overlay.CreateOverlay("overlay", "OL", ref overlayHandle);
        overlay.ShowOverlay(overlayHandle);
        if (error == EVROverlayError.None) return;
    }

    void destroyOverlay()
    {
        if (overlayHandle == OpenVR.k_ulOverlayHandleInvalid) return;
        var overlay = OpenVR.Overlay;
        if (overlay != null) overlay.DestroyOverlay(overlayHandle);
        overlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    }
}
