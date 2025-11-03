using System;
using UnityEngine;

public class TargetZone : MonoBehaviour
{
    public LayerMask cowLayerMask;

    private void Start()
    {
        var cowSpawner = FindFirstObjectByType<CowSpawner>();
        if (cowSpawner != null)
        {
            cowSpawner.gameObject.SetActive(true);
            cowSpawner.avoidCenter = transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & cowLayerMask) == 0) return;
        
        var cow = other.gameObject;
        cow.gameObject.SetActive(false);
        Destroy(cow, 0.5f);
        GameManager.Instance.OnKidnappedCow();
    }

    private void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
