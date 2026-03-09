using UnityEngine;
using UnityEngine.XR;

public class CrosshairController : MonoBehaviour
{
    public float speed = 500f;
    public float minX = -400f, maxX = 400f, minY = -200f, maxY = 200f;

    [Header("Audio")]
    public AudioSource movementSound;

    private RectTransform rectTrans;

    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
    }

    void Update()
    {
        // --- 1. RESTORED PC KEYBOARD FALLBACK ---
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // --- 2. VR CONTROLLER LOGIC ---
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightHand.isValid)
        {
            if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick))
            {
                // Override keyboard if VR thumbstick is actually being pushed
                if (Mathf.Abs(thumbstick.x) > 0.1f || Mathf.Abs(thumbstick.y) > 0.1f)
                {
                    moveX = thumbstick.x;
                    moveY = thumbstick.y;
                }
            }
        }

        // --- 3. AUDIO LOGIC ---
        bool isMoving = Mathf.Abs(moveX) > 0.05f || Mathf.Abs(moveY) > 0.05f;

        if (movementSound != null)
        {
            if (isMoving && !movementSound.isPlaying)
                movementSound.Play();
            else if (!isMoving && movementSound.isPlaying)
                movementSound.Stop();
        }

        // --- 4. MOVEMENT LOGIC ---
        Vector2 currentPos = rectTrans.anchoredPosition;
        Vector2 movement = new Vector2(moveX, moveY) * speed * Time.deltaTime;
        Vector2 newPos = currentPos + movement;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        rectTrans.anchoredPosition = newPos;
    }
}