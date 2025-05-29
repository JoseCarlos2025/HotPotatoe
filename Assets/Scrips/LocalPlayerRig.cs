// LocalPlayerRig.cs
using UnityEngine;

public class LocalPlayerRig : MonoBehaviour
{
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Hand Model Prefabs")]
    public GameObject defaultLeftHandPrefab;
    public GameObject defaultRightHandPrefab;

    private GameObject currentLeftHandModel;
    private GameObject currentRightHandModel;

    void Start()
    {
        // Instanciar modelos por defecto al inicio
        SetLeftHandModel(defaultLeftHandPrefab);
        SetRightHandModel(defaultRightHandPrefab);
    }

    void Update()
    {
        if (VRRigReferences.Singleton == null) return;

        root.position = VRRigReferences.Singleton.root.position;
        root.rotation = VRRigReferences.Singleton.root.rotation;

        head.position = VRRigReferences.Singleton.head.position;
        head.rotation = VRRigReferences.Singleton.head.rotation;

        leftHand.position = VRRigReferences.Singleton.leftHand.position;
        leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

        rightHand.position = VRRigReferences.Singleton.rightHand.position;
        rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
    }

    public void SetLeftHandModel(GameObject prefab)
    {
        if (currentLeftHandModel != null)
            Destroy(currentLeftHandModel);

        currentLeftHandModel = Instantiate(prefab, leftHand);
        currentLeftHandModel.transform.localPosition = Vector3.zero;
        currentLeftHandModel.transform.localRotation = Quaternion.identity;
    }

    public void SetRightHandModel(GameObject prefab)
    {
        if (currentRightHandModel != null)
            Destroy(currentRightHandModel);

        currentRightHandModel = Instantiate(prefab, rightHand);
        currentRightHandModel.transform.localPosition = Vector3.zero;
        currentRightHandModel.transform.localRotation = Quaternion.identity;
    }

    public void ResetLeftHandModel()
    {
        SetLeftHandModel(defaultLeftHandPrefab);
    }

    public void ResetRightHandModel()
    {
        SetRightHandModel(defaultRightHandPrefab);
    }
}
