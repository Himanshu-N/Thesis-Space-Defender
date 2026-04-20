using UnityEngine;
using UnityEngine.XR;

public class InstructionSequence : MonoBehaviour
{
    [Header("UI Pages")]
    [Tooltip("Drag your instruction panels here in order")]
    public GameObject[] instructionPages;

    private int currentPage = 0;
    private bool wasTriggerPressed = false;
    private float inputCooldown = 0.5f; // Prevents double-clicking instantly

    void Start()
    {
        ShowPage(0);
    }

    void Update()
    {
        if (inputCooldown > 0)
        {
            inputCooldown -= Time.deltaTime;
            return;
        }

        // VR Input
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggerPressed);

        bool tryingToContinue = Input.GetKeyDown(KeyCode.Space) || (isTriggerPressed && !wasTriggerPressed);
        wasTriggerPressed = isTriggerPressed;

        if (tryingToContinue) NextPage();
    }

    public void NextPage()
    {
        currentPage++;
        if (currentPage < instructionPages.Length)
        {
            ShowPage(currentPage);
            inputCooldown = 0.5f;
        }
        else
        {
            // Reached the end! Start the game and hide instructions.
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
            gameObject.SetActive(false);
        }
    }

    void ShowPage(int index)
    {
        for (int i = 0; i < instructionPages.Length; i++)
        {
            instructionPages[i].SetActive(i == index);
        }
    }
}