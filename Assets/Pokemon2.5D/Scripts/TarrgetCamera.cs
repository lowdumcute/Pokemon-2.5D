using UnityEngine;

public class TarrgetCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera View")]
    public Vector3 offset = new Vector3(0f, 12f, -12f);

    [Header("Smooth Follow")]
    [Range(0.01f, 1f)]
    public float smoothTime = 0.12f;

    [Header("View Angle")]
    [Range(10f, 80f)]
    public float tiltAngle = 45f;

    [Header("Occlusion")]
    public LayerMask obstacleLayer;

    [Header("Fade Settings")]
    public Material fadeMaterial;

    [Range(0f, 1f)]
    public float fadeAlpha = 0.3f;

    public float fadeSpeed = 8f;

    private Vector3 currentVelocity;

    private Renderer currentObstacle;

    private Material[] originalMaterials;

    private Material[] fadeMaterials;

    private float currentAlpha = 1f;

    private bool isFading;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                target = player.transform;
        }

        transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        FollowTarget();
        HandleOcclusion();
        UpdateFade();
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            smoothTime
        );

        transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
    }

    private void HandleOcclusion()
    {
        Vector3 direction = target.position - transform.position;

        float distance = Vector3.Distance(
            transform.position,
            target.position
        );

        Ray ray = new Ray(
            transform.position,
            direction.normalized
        );

        if (Physics.Raycast(ray, out RaycastHit hit, distance, obstacleLayer))
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();

            if (rend != null)
            {
                if (currentObstacle != rend)
                {
                    RestoreOldObstacle();

                    currentObstacle = rend;

                    originalMaterials = rend.materials;

                    fadeMaterials = new Material[rend.materials.Length];

                    for (int i = 0; i < fadeMaterials.Length; i++)
                    {
                        Material matInstance = new Material(fadeMaterial);

                        Color color = matInstance.color;
                        color.a = 1f;

                        matInstance.color = color;

                        fadeMaterials[i] = matInstance;
                    }

                    rend.materials = fadeMaterials;

                    currentAlpha = 1f;
                }

                isFading = true;
                return;
            }
        }

        isFading = false;
    }

    private void UpdateFade()
    {
        if (currentObstacle == null || fadeMaterials == null)
            return;

        float targetAlpha = isFading ? fadeAlpha : 1f;

        currentAlpha = Mathf.Lerp(
            currentAlpha,
            targetAlpha,
            Time.deltaTime * fadeSpeed
        );

        foreach (Material mat in fadeMaterials)
        {
            Color color = mat.color;
            color.a = currentAlpha;
            mat.color = color;
        }

        // Restore khi alpha gần về 1
        if (!isFading && currentAlpha >= 0.98f)
        {
            RestoreOldObstacle();
        }
    }

    private void RestoreOldObstacle()
    {
        if (currentObstacle != null)
        {
            currentObstacle.materials = originalMaterials;

            currentObstacle = null;
            fadeMaterials = null;
        }
    }
}