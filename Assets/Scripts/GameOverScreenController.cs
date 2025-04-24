using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreenController : MonoBehaviour {
    
    [SerializeField] private Material material;
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
        noise = noiseAmount;
        glitch = glitchStrength;
        scanLines = scanLinesStrength;
        color1 = scanLineColor;
        color2 = scanLineColor;
    }

    void Update()
    {
        material.SetFloat("_NoiseAmount", noise);
        material.SetFloat("_GlitchStrength", glitch);
        material.SetFloat("_ScanLinesStrength", scanLines);
        material.SetColor("_NoiseColor", color1);
        material.SetColor("_ScanLineColor", color2);
    }

    public void OnReviveClick() {
        SceneManager.LoadScene("GameScene");
    }
}