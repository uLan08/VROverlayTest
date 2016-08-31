using UnityEngine;
using System.Collections;
using System.Linq;
using Valve.VR;
using Random = System.Random;

public class Overlay : MonoBehaviour
{
    public static Random Rand = new Random();
    public static string Key { get { return "unity:" + Application.companyName + "." + Application.productName + "." + Rand.Next(); } } // A Unique Key for this application to spawn Overlays. This key must be unique within OpenVR. You should use a Key that is not likely to be used by any other application

    public GameObject OverlayReference { get { return _overlayReference ?? (_overlayReference = new GameObject("Overlay Reference") {hideFlags = HideFlags.HideInHierarchy}); } } // A Reference relative to the Overlay, used for position offsets
    private GameObject _overlayReference;
    public static GameObject ZeroReference; // A Reference relative to the World Center, used for position offsets
    
    public Texture OverlayTexture;
    public float Scale = 1f;

    public SteamVR_TrackedObject.EIndex AttachedToDevice = SteamVR_TrackedObject.EIndex.None;
    private SteamVR_TrackedObject.EIndex _lastAttached;

    [HideInInspector]
    public uint AnchorDeviceId
    {
        get { return _anchorDeviceId; }
        set
        {
            _anchorDeviceId = value;
            if (value == OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                OverlayReference.transform.parent = null;
                OverlayReference.transform.localPosition = Vector3.zero;
            }
            else
            {
                var objs = GameObject.FindObjectsOfType<SteamVR_TrackedObject>();
                foreach (var obj in objs.Where(obj => (uint) obj.index == value))
                {
                    OverlayReference.transform.parent = obj.gameObject.transform;
                    OverlayReference.transform.localPosition = Vector3.zero;
                    break;
                }
            }
        }
    }
    private uint _anchorDeviceId;

    public Vector4 UvOffset = new Vector4(0, 0, 1, 1);
    public Vector2 CurvedRange = new Vector2(1, 2);

    private ulong _overlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    // Keep track of Overlay Changes
    private bool _overlayChanged;
    private uint _lastAnchorDeviceId;
    private Texture _lastOverlayTexture;
    private Vector3 _lastOverlayPosition;
    private Quaternion _lastOverlayRotation;
    private Vector4 _lastUvOffset = new Vector4(0, 0, 1, 1);
    private Vector2 _lastCurvedRange = new Vector2(1, 2);

    public void OnEnable()
    {
        var steamvr = SteamVR.instance;
        CreateOverlay();
	    UpdateOverlay();
	}

    public void Update()
    {
        CheckOverlayChanged(ref _overlayChanged);
        if (_overlayChanged)
            UpdateOverlay(); // Only needs to be called when something changes! Do not call every frame unless necessary.
    }

    public void OnDisable()
    {
        DestroyOverlay();
    }

    /// <summary>
    /// Check if the Overlay has changed in the last frame
    /// </summary>
    /// <param name="changed"></param>
    private void CheckOverlayChanged(ref bool changed)
    {
        if (AttachedToDevice == _lastAttached &&
            AnchorDeviceId == _lastAnchorDeviceId &&
            OverlayTexture == _lastOverlayTexture &&
            _lastOverlayPosition == gameObject.transform.position &&
            _lastOverlayRotation == gameObject.transform.rotation &&
            UvOffset == _lastUvOffset &&
            CurvedRange == _lastCurvedRange) return;

        _lastAttached = AttachedToDevice;
        AnchorDeviceId = (uint) AttachedToDevice;
        _lastAnchorDeviceId = AnchorDeviceId;
        _lastOverlayTexture = OverlayTexture;
        _lastOverlayPosition = gameObject.transform.position;
        _lastOverlayRotation = gameObject.transform.rotation;
        _lastUvOffset = UvOffset;
        _lastCurvedRange = CurvedRange;
        changed = true;
    }

    /// <summary>
    /// Create an Overlay within SteamVR
    /// </summary>
    private void CreateOverlay()
    {
        var overlay = OpenVR.Overlay;
        if (overlay == null) return;

        var error = overlay.CreateOverlay(Key + gameObject.GetInstanceID(), gameObject.name, ref _overlayHandle); // This key must be unique within OpenVR. You should use a Key that is not likely to be used by any other application.
        if (error == EVROverlayError.None) return;
        Debug.LogError(error.ToString());
        enabled = false;
    }

    private static bool ComputeIntersection(ulong handle, Vector3 source, Vector3 direction, ref SteamVR_Overlay.IntersectionResults results)
    {
        var overlay = OpenVR.Overlay;
        if (overlay == null) return false;

        var input = new VROverlayIntersectionParams_t
        {
            eOrigin = SteamVR_Render.instance.trackingSpace,
            vSource =
            {
                v0 = source.x,
                v1 = source.y,
                v2 = -source.z
            },
            vDirection =
            {
                v0 = direction.x,
                v1 = direction.y,
                v2 = -direction.z
            }
        };

        var output = new VROverlayIntersectionResults_t();
        if (!overlay.ComputeOverlayIntersection(handle, ref input, ref output)) return false;

        results.point = new Vector3(output.vPoint.v0, output.vPoint.v1, -output.vPoint.v2);
        results.normal = new Vector3(output.vNormal.v0, output.vNormal.v1, -output.vNormal.v2);
        results.UVs = new Vector2(output.vUVs.v0, output.vUVs.v1);
        results.distance = output.fDistance;
        return true;
    }

    public Vector2 getUVs(Vector3 source, Vector3 direction)
    {
        var result = new SteamVR_Overlay.IntersectionResults();
        var hit = ComputeIntersection(_overlayHandle, source, direction, ref result);
        Debug.Log(hit);
        return result.UVs;
    }

    /// <summary>
    /// Update the Overlay
    /// </summary>
    private void UpdateOverlay()
    {
        var overlay = OpenVR.Overlay;
        if (overlay == null) return;

        if (OverlayTexture != null) // Texture set
        {
            var error = overlay.ShowOverlay(_overlayHandle); // Show Overlay
            if (error == EVROverlayError.InvalidHandle || error == EVROverlayError.UnknownOverlay) // Overlay handle invalid
            {
                if (overlay.FindOverlay(Key + gameObject.GetInstanceID(), ref _overlayHandle) != EVROverlayError.None) // Attempt to grab handle by name
                    return;
            }
            
            // Setup a Native texture
            var tex = new Texture_t
            {
                handle = OverlayTexture.GetNativeTexturePtr(),
                eType = SteamVR.instance.graphicsAPI,
                eColorSpace = EColorSpace.Auto
            };

            overlay.SetOverlayColor(_overlayHandle, 1f, 1f, 1f); // Assign Overlay Color
            overlay.SetOverlayTexture(_overlayHandle, ref tex); // Assign Overlay Texture
            overlay.SetOverlayAlpha(_overlayHandle, 1f); // Assign Overlay Alpha
            overlay.SetOverlayWidthInMeters(_overlayHandle, Scale); // Assign Overlay Scale (height is automatic to match texture aspect)
            overlay.SetOverlayAutoCurveDistanceRangeInMeters(_overlayHandle, CurvedRange.x, CurvedRange.y);

            // Setup Texture bounds
            var textureBounds = new VRTextureBounds_t
            {
                uMin = (0 + UvOffset.x) * UvOffset.z,
                vMin = (1 + UvOffset.y) * UvOffset.w,
                uMax = (1 + UvOffset.x) * UvOffset.z,
                vMax = (0 + UvOffset.y) * UvOffset.w
            };
            overlay.SetOverlayTextureBounds(_overlayHandle, ref textureBounds);

            var vecMouseScale = new HmdVector2_t
            {
                v0 = 1f,
                v1 = (float)OverlayTexture.height / (float)OverlayTexture.width
            };
            overlay.SetOverlayMouseScale(_overlayHandle, ref vecMouseScale);

            // Attach Overlay to relative position
            if (AnchorDeviceId != OpenVR.k_unTrackedDeviceIndexInvalid) // Attach to some device (like a controller)
            {
                var t = GetOverlayPosition();
                overlay.SetOverlayTransformTrackedDeviceRelative(_overlayHandle, AnchorDeviceId, ref t);
            }
            else // Attach to position in world
            {
                var t = GetOverlayPosition();
                overlay.SetOverlayTransformAbsolute(_overlayHandle, SteamVR_Render.instance.trackingSpace, ref t);
            }

            overlay.SetOverlayInputMethod(_overlayHandle, VROverlayInputMethod.None);
        }
        else // Texture not set, hide overlay
        {
            overlay.HideOverlay(_overlayHandle);
        }
    }

    /// <summary>
    /// Destroy the Overlay within SteamVR
    /// </summary>
    private void DestroyOverlay()
    {
        if (_overlayHandle == OpenVR.k_ulOverlayHandleInvalid) return;
        var overlay = OpenVR.Overlay;
        if (overlay != null) overlay.DestroyOverlay(_overlayHandle);
        _overlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    }

    /// <summary>
    /// Get an HmdMatrix that is equivalent to this Overlay's position in space
    /// </summary>
    /// <returns></returns>
    private HmdMatrix34_t GetOverlayPosition()
    {
        if (AnchorDeviceId == OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            var offset = new SteamVR_Utils.RigidTransform(OverlayReference.transform, transform);
            offset.pos.x /= OverlayReference.transform.localScale.x;
            offset.pos.y /= OverlayReference.transform.localScale.y;
            offset.pos.z /= OverlayReference.transform.localScale.z;
            var t = offset.ToHmdMatrix34();
            return t;
        }
        else
        {
            if (ZeroReference == null) ZeroReference = new GameObject("Zero Reference") { hideFlags = HideFlags.HideInHierarchy };
            var offset = new SteamVR_Utils.RigidTransform(ZeroReference.transform, transform);
            offset.pos.x /= ZeroReference.transform.localScale.x;
            offset.pos.y /= ZeroReference.transform.localScale.y;
            offset.pos.z /= ZeroReference.transform.localScale.z;
            var t = offset.ToHmdMatrix34();
            return t;
        }
    }
}
