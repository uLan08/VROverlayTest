using UnityEngine;
using System.Collections;

public class TrackedDeviceManager : MonoBehaviour {

    private SteamVR_TrackedObject hmd;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void FindTracker(ref SteamVR_TrackedObject tracker, SteamVR_TrackedObject.EType type)
    {
        if (tracker != null && tracker.isValid) return;
        // Try to find an HOTK_TrackedDevice that is active and tracking the HMD
        foreach (var g in FindObjectsOfType<SteamVR_TrackedObject>())
        {
            if(g.enabled && g.type == type)
            {
                tracker = g;
                break;
            }
        }

        if (tracker != null) return;
        Debug.LogWarning("Couldn't find a " + type.ToString() + " tracker. Making one up :(");
        var go = new GameObject(type.ToString() + " Tracker", typeof(SteamVR_TrackedObject)) { hideFlags = HideFlags.HideInHierarchy }.GetComponent<SteamVR_TrackedObject>();
        go.type = type;
        tracker = go;
    }

    public SteamVR_TrackedObject getHMDReference()
    {
        FindTracker(ref hmd, SteamVR_TrackedObject.EType.HMD);
        return hmd;
    }
}
