using System;
using UnityEngine;

public class UFO : MonoBehaviour
{
    public bool beamEnabled = false;
    public GameObject beam;
    public LayerMask layerMask;
    public Transform CowTractionTransform;
    private Rigidbody cowRb;
    private Transform cow;
    private float hoverTime;

    public GameObject outlineObject;
    public GameObject normalObject;
    
    private float wiggleTime;

    private void Start()
    {
        EnableBeam(true);
        Deselect();
    }

    public void EnableBeam(bool enable)
    {
        beamEnabled = enable;
        beam.SetActive(enable);
        if (!enable && cowRb)
        {
            cowRb.useGravity = true;
            cowRb.isKinematic = false;
            cow = null;
            cowRb = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!beamEnabled) return;
        if (((1 << other.gameObject.layer) & layerMask) != 0)
        {
            cow = other.transform;
            cowRb = other.attachedRigidbody;
            cowRb.useGravity = false;
            cowRb.isKinematic = true;
            cow.position = Vector3.Lerp(cow.position, CowTractionTransform.position, Time.deltaTime * 2f);
            hoverTime += Time.deltaTime * 2f;
            float hover = Mathf.Sin(hoverTime) * 0.001f;
            cow.position += new Vector3(0, hover, 0);
            cow.rotation = Quaternion.Euler(0, Mathf.Sin(hoverTime) * 15f, 0);
        }
    }
    
    void Update()
    {
        wiggleTime += Time.deltaTime * 2f;
        float wiggleY = Mathf.Sin(wiggleTime) * 0.01f;
        float wiggleX = Mathf.Cos(wiggleTime * 0.7f) * 0.006f;
        transform.position += new Vector3(wiggleX, wiggleY, 0) * Time.deltaTime;
        transform.rotation = Quaternion.Euler(
            Mathf.Sin(wiggleTime * 0.5f) * 2f,
            transform.rotation.eulerAngles.y,
            Mathf.Cos(wiggleTime * 0.3f) * 2f
        );
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (cow && other.transform == cow)
        {
            cowRb.useGravity = true;
            cowRb.isKinematic = false;
            cow = null;
            cowRb = null;
        }
    }

    public void Select()
    {
        outlineObject.SetActive(true);
        normalObject.SetActive(false);
        EnableBeam(true);
    }

    public void Deselect()
    {
        outlineObject.SetActive(false);
        normalObject.SetActive(true);
        EnableBeam(false);
    }
}