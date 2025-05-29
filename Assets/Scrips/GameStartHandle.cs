using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GameStartHandle : MonoBehaviour
{
    public GameManager gameManager;

    XRGrabInteractable grabInteractable;

    public GameObject punterDerecho;
    public GameObject punterIzquierdo;

    public HotPotato hotPotato;
    public GameObject LhandModelGrabbing;
    public GameObject RhandModelGrabbing;

    private bool isPunterEnabled;
    private LocalPlayerRig localPlayerRig;

    void Awake()
    {
        isPunterEnabled = PlayerPrefs.GetInt("PunterEnabled", 1) == 1;

        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        localPlayerRig = FindAnyObjectByType<LocalPlayerRig>();
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        bool isLeft = args.interactorObject.handedness.ToString() == "Left";

        if (isLeft)
        {
            hotPotato.holdingNode = XRNode.LeftHand;
            if (isPunterEnabled) punterDerecho.SetActive(false);
            if (localPlayerRig != null && LhandModelGrabbing != null)
                localPlayerRig.SetLeftHandModel(LhandModelGrabbing);
        }
        else
        {
            hotPotato.holdingNode = XRNode.RightHand;
            if (isPunterEnabled) punterIzquierdo.SetActive(false);
            if (localPlayerRig != null && RhandModelGrabbing != null)
                localPlayerRig.SetRightHandModel(RhandModelGrabbing);
        }

        AudioManager.instance?.PlaySFX("sword");
        gameManager?.StartOrResumeGame();
    }

    void OnReleased(SelectExitEventArgs args)
    {
        bool isLeft = args.interactorObject.handedness.ToString() == "Left";

        if (isPunterEnabled)
        {
            if (isLeft) punterDerecho.SetActive(true);
            else punterIzquierdo.SetActive(true);
        }

        if (localPlayerRig != null)
        {
            if (isLeft) localPlayerRig.ResetLeftHandModel();
            else localPlayerRig.ResetRightHandModel();
        }

        gameManager?.PauseGame();
    }
}
