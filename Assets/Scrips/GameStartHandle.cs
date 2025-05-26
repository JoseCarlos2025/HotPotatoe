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

    private bool isPunterEnabled;

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
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (args.interactorObject.handedness.ToString() == "Left")
        {
            hotPotato.holdingNode = XRNode.LeftHand;
            if (isPunterEnabled)
            {
                punterDerecho.SetActive(false);
            }
        }
        else if (args.interactorObject.handedness.ToString() == "Right")
        {
            hotPotato.holdingNode = XRNode.RightHand;
            if (isPunterEnabled)
            {
                punterIzquierdo.SetActive(false);
            }
        }

        AudioManager.instance?.PlaySFX("sword");
        gameManager?.StartOrResumeGame();
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (isPunterEnabled)
        {
            if (args.interactorObject.handedness.ToString() == "Left")
            {
                punterDerecho.SetActive(true);
            }
            else if (args.interactorObject.handedness.ToString() == "Right")
            {
                punterIzquierdo.SetActive(true);
            }
        }

        gameManager?.PauseGame();
    }
}

