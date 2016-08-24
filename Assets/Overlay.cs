using UnityEngine;
using System.Collections;
using Valve.VR;

public class Overlay : MonoBehaviour {

    public Texture overlayTexture;
    ulong overlayHandle = OpenVR.k_ulOverlayHandleInvalid;

	void Start ()
    {
	
	}

   public void createOverlay()
    {
        var steamvr = SteamVR.instance;
        var overlay = OpenVR.Overlay;
        if (overlay == null) return;
        var error = overlay.CreateOverlay("overlay", "OL", ref overlayHandle);
        overlay.ShowOverlay(overlayHandle);
        if (error == EVROverlayError.None) return;
    }

    public void destroyOverlay()
    {
        if (overlayHandle == OpenVR.k_ulOverlayHandleInvalid) return;
        var overlay = OpenVR.Overlay;
        if (overlay != null) overlay.DestroyOverlay(overlayHandle);
        overlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    }
}
