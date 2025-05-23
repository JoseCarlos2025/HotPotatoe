using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class BillboardSoloY : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {

        if (Camera.main != null)
            camTransform = Camera.main.transform;
        else
            Debug.LogWarning("No se encontró una cámara principal (MainCamera) en la escena.");
    }

    void Update()
    {
        if (camTransform == null)
            return;

        Vector3 direction = camTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = targetRotation;
    }
}

