using UnityEngine;

namespace Entities.Nuri
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TargetOutline : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Material _outlineMaterial;
        private Material _originalMaterial;
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineSize = Shader.PropertyToID("_OutlineSize");

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalMaterial = _spriteRenderer.material;
        }

        public void EnableOutline(Color color, float size)
        {
            if (_outlineMaterial == null)
            {
                _outlineMaterial = new Material(Shader.Find("Sprites/Outline"));
            }

            _spriteRenderer.material = _outlineMaterial;
            _outlineMaterial.SetColor(OutlineColor, color);
            _outlineMaterial.SetFloat(OutlineSize, size);
        }

        public void DisableOutline()
        {
            if (_spriteRenderer != null && _originalMaterial != null)
            {
                _spriteRenderer.material = _originalMaterial;
            }
        }

        private void OnDestroy()
        {
            if (_outlineMaterial != null)
            {
                Destroy(_outlineMaterial);
            }
        }
    }
}
