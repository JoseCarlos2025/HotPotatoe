using UnityEngine;

public class LocalXRInput : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform leftHandTransform;
    public Transform rightHandTransform;

    private Transform netHead;
    private Transform netLeftHand;
    private Transform netRightHand;

    public void AssignTargets(Transform head, Transform leftHand, Transform rightHand)
    {
        netHead = head;
        netLeftHand = leftHand;
        netRightHand = rightHand;
    }

    void Update()
    {
        if (netHead != null)
        {
            netHead.position = cameraTransform.position;
            netHead.rotation = cameraTransform.rotation;

            netLeftHand.position = leftHandTransform.position;
            netLeftHand.rotation = leftHandTransform.rotation;

            netRightHand.position = rightHandTransform.position;
            netRightHand.rotation = rightHandTransform.rotation;
        }
    }
}

