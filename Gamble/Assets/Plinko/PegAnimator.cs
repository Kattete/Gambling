using UnityEngine;
using System.Collections;

public class PegAnimator : MonoBehaviour
{
    // Animation settings
    [Header("Scale Animation")]
    [SerializeField] private float scaleMultiplier = 1.5f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Color hitColor = Color.green;

    // Partical system reference
    [Header("Particles")]
    [SerializeField] private ParticleSystem collisionParticles;

    // Store the original values
    private Vector3 originalScale;
    private Color originalColor;
    private Material pegMaterial;

    private void Start()
    {
        originalScale = transform.localScale;
        pegMaterial = GetComponent<Renderer>().material;
        originalColor = pegMaterial.color;

        if(collisionParticles == null)
        {
            CreateParticleSystem();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is ball
        if (collision.gameObject.CompareTag("Ball"))
        {
            StartCoroutine(AnimateHit());

            // Play particle effect
            collisionParticles.transform.position = collision.contacts[0].point;
            collisionParticles.Play();
        }
    }

    private IEnumerator AnimateHit()
    {
        float elapsedTime = 0f;

        // Scale up and change color
        while (elapsedTime < animationDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (animationDuration / 2);

            // Smoothly scale up
            transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleMultiplier, progress);

            // Smoothly change color
            pegMaterial.color = Color.Lerp(originalColor, hitColor, progress);

            yield return null;
        }

        // Scale back down and restore color
        elapsedTime = 0f;
        while (elapsedTime < animationDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (animationDuration / 2);

            transform.localScale = Vector3.Lerp(originalScale * scaleMultiplier, originalScale, progress);
            pegMaterial.color = Color.Lerp(hitColor, originalColor, progress);

            yield return null;
        }

        // Ensure we end up exactly at the original values
        transform.localScale = originalScale;
        pegMaterial.color = originalColor;
    }

    private void CreateParticleSystem()
    {
        // Create a new GameObject for our particle system
        GameObject particleObj = new GameObject("CollisionParticles");
        particleObj.transform.SetParent(transform);

        // Add and configure the particle system
        collisionParticles = particleObj.AddComponent<ParticleSystem>();
        var main = collisionParticles.main;
        collisionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        main.duration = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.startColor = hitColor;

        // Configure particle system shape
        var shape = collisionParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        // Configure emission
        var emission = collisionParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 10));

        // Make particles fade out
        var colorOverLifetime = collisionParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        // Create a gradient for fading
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(hitColor, 0.0f), new GradientColorKey(hitColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
    }
}
