using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreenController : MonoBehaviour {
    private static readonly int NoiseAmount = Shader.PropertyToID("_NoiseAmount");
    private static readonly int GlitchStrength = Shader.PropertyToID("_GlitchStrength");
    private static readonly int ScanLinesStrength = Shader.PropertyToID("_ScanLinesStrength");
    private static readonly int NoiseColor = Shader.PropertyToID("_NoiseColor");
    private static readonly int ScanLineColor = Shader.PropertyToID("_ScanLineColor");

    [SerializeField] private Material material;
    [SerializeField] private float noiseAmount = 200;
    [SerializeField] private float glitchStrength = 50;
    [SerializeField] private float scanLinesStrength;
    [SerializeField] private Color scanLineColor = new Color(0.7372549019607844f,0.5803921568627451f,0.7647058823529411f);
    
    private float _noise;
    private float _glitch;
    private float _scanLines;
    private Color _color1;
    private Color _color2;

    public void Start() {
        _noise = noiseAmount;
        _glitch = glitchStrength;
        _scanLines = scanLinesStrength;
        _color1 = scanLineColor;
        _color2 = scanLineColor;
    }

    void Update()
    {
        material.SetFloat(NoiseAmount, _noise);
        material.SetFloat(GlitchStrength, _glitch);
        material.SetFloat(ScanLinesStrength, _scanLines);
        material.SetColor(NoiseColor, _color1);
        material.SetColor(ScanLineColor, _color2);
    }

    public void OnReviveClick() {
        SceneManager.LoadScene("GameScene");
    }
}