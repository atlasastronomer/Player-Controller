using Entities.Player;
using UnityEngine;

namespace Core.Utils
{
    public class GlitchController : MonoBehaviour
    {
        private static readonly int NoiseAmount = Shader.PropertyToID("_NoiseAmount");
        private static readonly int GlitchStrength = Shader.PropertyToID("_GlitchStrength");
        private static readonly int ScanLinesStrength = Shader.PropertyToID("_ScanLinesStrength");
        private static readonly int ScanLineColor = Shader.PropertyToID("_ScanLineColor");
        private static readonly int NoiseColor = Shader.PropertyToID("_NoiseColor");

        [SerializeField] private Material material;
        [SerializeField] private PlayerHealth playerHealth;

        [SerializeField] private float noiseAmount = 200;
        [SerializeField] private float glitchStrength = 50;
        [SerializeField] private float scanLinesStrength;
        [SerializeField] private Color noiseColor = new Color(1.0f, 0.47843137254901963f, 0.9607843137254902f);

        [SerializeField]
        private Color scanLineColor = new Color(0.7372549019607844f, 0.5803921568627451f, 0.7647058823529411f);
        // private Color scanLineColor = new Color(0.6705882352941176f,0.6705882352941176f,0.6705882352941176f);

        private float _noise;
        private float _glitch;
        private float _scanLines;
        private Color _color1;
        private Color _color2;

        private float _glitchEffectTime;

        public void Start()
        {
            _noise = 0;
            _glitch = 0;
            _scanLines = 20;
            _color1 = Color.white;
            _color2 = Color.white;

            PlayerHealth.OnDamageTaken += SetGlitchTimer;
            PlayerHealth.OnDeath += SetGlitchTimer;
        }

        private void SetGlitchTimer()
        {
            _glitchEffectTime = playerHealth.GracePeriodCooldown;
        }

        void Update()
        {
            _glitchEffectTime -= Time.deltaTime;
            if (_glitchEffectTime >= 0)
            {
                _noise = noiseAmount;
                _glitch = glitchStrength;
                _scanLines = scanLinesStrength;
                _color1 = noiseColor;
                _color2 = scanLineColor;
            }
            else
            {
                _noise = 0;
                _glitch = 0;
                _scanLines = 20;
                _color1 = Color.white;
                _color2 = Color.white;
            }

            material.SetFloat(NoiseAmount, _noise);
            material.SetFloat(GlitchStrength, _glitch);
            material.SetFloat(ScanLinesStrength, _scanLines);
            material.SetColor(NoiseColor, _color1);
            material.SetColor(ScanLineColor, _color2);
        }
    }
}