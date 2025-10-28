using UnityEngine;

/// <summary>
/// Component to handle ray rendering from UFO saucer to ground when touched.
/// </summary>
public class UFORay : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField] float rayLength = 10f;
    [SerializeField] Color rayColor = Color.cyan;
    [SerializeField] float rayWidth = 0.02f;

    LineRenderer lineRenderer;
    Camera mainCamera;
    bool isPressed = false;

    /// <summary>
    /// Initialize components.
    /// </summary>
    void Start()
    {
        mainCamera = Camera.main;

        // Create a LineRenderer for the ray
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.enabled = false; // hidden by default
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = rayColor;
    }

    /// <summary>
    /// Update is called once per frame to handle touch input and ray rendering.
    /// </summary>
    void Update()
    {
        HandleTouch();
        if (isPressed)
            UpdateRay();
    }

    /// <summary>
    /// Handle touch input to detect interaction with the UFO saucer.
    /// </summary>
    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            if (IsTouchedThisSaucer(touch.position))
            {
                InteractionFlag.isInteracting = true;
                isPressed = true;
                lineRenderer.enabled = true;
            }
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isPressed = false;
            lineRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Check if the touch position intersects with this UFO saucer with a raycast.
    /// </summary>
    bool IsTouchedThisSaucer(Vector2 touchPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(touchPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.transform == transform;
        return false;
    }

    /// <summary>
    /// Update the ray's start and end positions.
    /// </summary>
    void UpdateRay()
    {
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * rayLength;

        // DDetect the ground (AR plane or collider)
        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, rayLength))
            end = hit.point;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}
