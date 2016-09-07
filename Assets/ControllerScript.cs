using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControllerScript : MonoBehaviour {

    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;

    public Overlay overlay;
    public GameObject canvas;
    public Sprite cursorSprite;
    public GameObject HMD;
    public Material skyboxMat;

    GameObject cursor;
    GameObject yesButton;
    GameObject noButton;
    Color normalColor;
    Color highlightedColor;
    Color pressedColor;
    bool hasOverlay = false;
    bool inYes;
    bool inNo;
    float canvasWidth;
    float canvasHeight;
    Vector3 yesButtonPos;
    Rect yesButtonRect;
    Vector3 noButtonPos;
    Rect noButtonRect;
    float x;
    float y;


    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start()
    {
        cursor = new GameObject("cursor");
        normalColor = new Color32(45, 77, 170, 255);
        highlightedColor = new Color32(135, 166, 255, 255);
        pressedColor = new Color32(0, 255, 2, 255);
        Image image = cursor.AddComponent<Image>();
        image.sprite = cursorSprite;

        cursor.transform.SetParent(canvas.transform);
        cursor.transform.localPosition = Vector3.zero;
        cursor.transform.localRotation = Quaternion.identity;
        cursor.transform.localScale = Vector3.one / 5;

        yesButton = GameObject.FindGameObjectWithTag("Yes");
        noButton = GameObject.FindGameObjectWithTag("No");
        overlay.gameObject.SetActive(false);
        canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
        yesButtonPos = yesButton.transform.localPosition;
        yesButtonRect = yesButton.GetComponent<RectTransform>().rect;
        noButtonPos = noButton.transform.localPosition;
        noButtonRect = noButton.GetComponent<RectTransform>().rect;
        inYes = false;
        inNo = false;


    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.System))
        {
            if (!overlay.gameObject.activeSelf)
            {
                overlay.gameObject.SetActive(true);
                overlay.transform.position = HMD.gameObject.transform.position + (HMD.transform.forward * 3f);
                overlay.transform.rotation = Quaternion.Euler(0f, HMD.transform.eulerAngles.y, 0f);
                //RenderSettings.skybox = skyboxMat;
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
            if (inYes)
            {
                yesButton.GetComponent<Button>().image.color = Color.green;
                Debug.Log("pressed yes");
           
            }
            else if (inNo)
            {
                noButton.GetComponent<Button>().image.color = pressedColor;
                Debug.Log("pressed no");
                overlay.gameObject.SetActive(false);
            }
        }
        if (hasOverlay)
        {
            var uvs = overlay.getUVs(gameObject.transform.position, gameObject.transform.forward);
            x = uvs.x * canvasWidth;
            y = (1 - uvs.y) * canvasHeight;

            if (x != 0 && y != canvasHeight)
            {
                x -= (canvasWidth/2);
                y -= (canvasHeight/2);
            }
            Vector2 pos = new Vector2(x, y);
            cursor.transform.localPosition = pos;
        }
        if ((x > (yesButtonPos.x - (yesButtonRect.width / 2) ) && x < (yesButtonPos.x + (yesButtonRect.width/2))) && (y < (yesButtonPos.y + (yesButtonRect.height/2)) && y > (yesButtonPos.y - (yesButtonRect.height/2))))
        {
            yesButton.GetComponent<Button>().image.color = highlightedColor;
            inYes = true;
        }
        else if ((x > (noButtonPos.x - (noButtonRect.width / 2)) && x < (noButtonPos.x + (noButtonRect.width / 2))) && (y < (noButtonPos.y + (noButtonRect.height / 2)) && y > (noButtonPos.y - (noButtonRect.height / 2))))
        {
            noButton.GetComponent<Button>().image.color = highlightedColor;
            inNo = true;
        }
        else
        {
            resetButtons();
        }
    }

    private void resetButtons()
    {
        yesButton.GetComponent<Button>().image.color = normalColor;
        noButton.GetComponent<Button>().image.color = normalColor;
        inYes = false;
        inNo = false;
    }
}
