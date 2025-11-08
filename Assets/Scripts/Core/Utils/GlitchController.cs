using UnityEngine;

public class GlitchController : MonoBehaviour {
    private static readonly int NoiseAmount = Shader.PropertyToID("_NoiseAmount");
    private static readonly int GlitchStrength = Shader.PropertyToID("_GlitchStrength");
    private static readonly int ScanLinesStrength = Shader.PropertyToID("_ScanLinesStrength");
    private static readonly int ScanLineColor = Shader.PropertyToID("_ScanLineColor");
    private static readonly int NoiseColor = Shader.PropertyToID("_NoiseColor");

    [SerializeField] private Material material;
    [SerializeField] private PlayerHealth playerHealth;
    
    [SerializeField] private float noiseAmount = 200;
    [SerializeField] private float glitchStrength = 50;
    [SerializeField] private float scanLinesStrength = 0;
    [SerializeField] private Color noiseColor = new Color(1.0f,0.47843137254901963f,0.9607843137254902f);
    [SerializeField] private Color scanLineColor = new Color(0.7372549019607844f,0.5803921568627451f,0.7647058823529411f);
    // private Color scanLineColor = new Color(0.6705882352941176f,0.6705882352941176f,0.6705882352941176f);
    
    private float noise;
    private float glitch;
    private float scanLines;
    private Color color1;
    private Color color2;
    
    private float glitchEffectTime = 0;

    public void Start() {
        noise = 0;
        glitch = 0;
        scanLines = 20;
        color1 = Color.white;
        color2 = Color.white;
        
        PlayerHealth.OnDamageTaken += SetGlitchTimer;
        PlayerHealth.OnDeath += SetGlitchTimer;
    }

    private void SetGlitchTimer() {
        glitchEffectTime = playerHealth.gracePeriodCooldown;
    }

    void Update()
    {
        glitchEffectTime -= Time.deltaTime;
        if (glitchEffectTime >= 0) {
            noise = noiseAmount;
            glitch = glitchStrength;
            scanLines = scanLinesStrength;
            color1 = noiseColor;
            color2 = scanLineColor;
        }
        else {
            noise = 0;
            glitch = 0;
            scanLines = 20;
            color1 = Color.white;
            color2 = Color.white;
        }
        
        material.SetFloat(NoiseAmount, noise);
        material.SetFloat(GlitchStrength, glitch);
        material.SetFloat(ScanLinesStrength, scanLines);
        material.SetColor(NoiseColor, color1);
        material.SetColor(ScanLineColor, color2);
    }
}
