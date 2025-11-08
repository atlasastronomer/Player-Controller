using System;
using UnityEngine;

public class Gem : MonoBehaviour, IItem {
    public static event Action OnGemCollect;
    
    public void Collect() {
        OnGemCollect?.Invoke();
        gameObject.SetActive(false);
    }
} 
