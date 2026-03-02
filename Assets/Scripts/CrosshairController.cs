using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public float speed = 500f;

    // Limits to keep crosshair on the glass
    public float minX = -400f;
    public float maxX = 400f;
    public float minY = -200f;
    public float maxY = 200f;

    private RectTransform rectTrans;

    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 1. Get Input (Arrow Keys or Left Joystick)
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left Stick
        float moveY = Input.GetAxis("Vertical");   // W/S or Left Stick

        // 2. Calculate new position
        Vector2 currentPos = rectTrans.anchoredPosition;
        Vector2 movement = new Vector2(moveX, moveY) * speed * Time.deltaTime;

        Vector2 newPos = currentPos + movement;

        // 3. Clamp (Limit) the position so it doesn't go off-screen
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        // 4. Apply
        rectTrans.anchoredPosition = newPos;
    }
}