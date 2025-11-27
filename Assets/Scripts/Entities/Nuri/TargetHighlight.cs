using UnityEngine;

public class TargetHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    private SpriteRenderer _spriteRenderer; 
    [SerializeField] private Color highlightColor = new Color(0.8f, 0.95f, 1f, 1f);
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseStrength = 0.5f;

    [Header("Optional Particles")]
    [SerializeField] private ParticleSystem highlightParticles;

    private Color _originalColor;
    private bool _isHighlighted;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    private void Update()
    {
        if (_isHighlighted)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            Color c = Color.Lerp(highlightColor, highlightColor + Color.white * pulseStrength, pulse);
            _spriteRenderer.color = c;
        }
    }

    public void EnableHighlight()
    {
        _isHighlighted = true;

        if (highlightParticles)
            highlightParticles.Play();
    }

    public void DisableHighlight()
    {
        _isHighlighted = false;
        _spriteRenderer.color = _originalColor;

        if (highlightParticles)
            highlightParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }
}
