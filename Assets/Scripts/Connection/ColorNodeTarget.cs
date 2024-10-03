using System;
using Events;
using UnityEngine;

namespace Connection
{
    public class ColorNodeTarget : MonoBehaviour
    {
        [SerializeField] private Color targetColor;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ColorNode colorNode;
        
        public bool IsCompleted => targetColor == colorNode.Color;

        public event Action<ColorNodeTarget, bool> TargetCompletionChangeEvent;


        private void Awake()
        {
            colorNode.ColorChangedEvent += OnColorChanged;
        }

        private void OnDestroy()
        {
            colorNode.ColorChangedEvent -= OnColorChanged;
        }

        private void OnColorChanged(Color currentColor)
        {
            // не понял, как подогнать цвета чтобы они на 100% совпадали,
            // поэтому допустил небольшую погрешность
            var delta = 
                Mathf.Abs(targetColor.r - currentColor.r) + 
                Mathf.Abs(targetColor.g - currentColor.g) + 
                Mathf.Abs(targetColor.b - currentColor.b); 
            TargetCompletionChangeEvent?.Invoke(this, delta <= 0.01f);
        }

        private void OnValidate()
        {
            spriteRenderer.color = targetColor;
        }
    }
}
