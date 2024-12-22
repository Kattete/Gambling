using UnityEngine;

public class BallTrailEffect : MonoBehaviour
{
    private TrailRenderer trailRenderer;
    [SerializeField] private Material trailMaterial;

    private void Start()
    {
        // Get or add the trail Renderer
        trailRenderer = GetComponent<TrailRenderer>();
        if(trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }

        // Configure The trails appereance
        trailRenderer.startWidth = 0.2f; // width at the start of the trail
        trailRenderer.endWidth = 0.05f; // width at the of the trail
        trailRenderer.time = 0.3f; // how long the trail persists

        if (trailMaterial != null) {
            trailRenderer.material = trailMaterial;
        }

        // Create a gradient for the trail color
        Gradient gradient = new Gradient();
        // Define silver colors for gradient
        Color brightSilver = new Color(0.98f, 0.98f, 0.98f);
        Color midSilver = new Color(0.85f, 0.85f, 0.85f);
        Color darkSilver = new Color(0.75f, 0.75f, 0.75f);
        gradient.SetKeys(new GradientColorKey[] {
            new GradientColorKey(brightSilver, 0.0f), new GradientColorKey(midSilver, 0.5f), new GradientColorKey(darkSilver, 1.0f)
        },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(0.7f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) });
        trailRenderer.colorGradient = gradient;

    }

    private void Update()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) {
            // Make trail brighter when moving faster
            float speed = rb.linearVelocity.magnitude;
            float brightness = Mathf.Lerp(0.75f, 1f, speed / 10f);

            // Update the trails brightness
            TrailRenderer trail = GetComponent<TrailRenderer>();
            Material material = trail.material;
            material.SetFloat("_Intensity", brightness);
        }
    }
}
