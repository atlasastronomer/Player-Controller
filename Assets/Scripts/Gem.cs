using System;
using UnityEngine;

public class Gem : MonoBehaviour, iItem {
    public static event Action OnGemCollect;
    
    public void Collect() {
        OnGemCollect?.Invoke();
        Destroy(gameObject);
    }
} 