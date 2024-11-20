using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using R3;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;
    
    [SerializeField] private GameObject healParticlePrefab;
    
    public void HealParticle(Vector3 pos)
    {
        Instantiate(healParticlePrefab, pos, Quaternion.identity);
    }
    
    public void HealParticleToPlayer()
    {
        var pos = new Vector3(-5.7f, 3.1f, 0);
        Instantiate(healParticlePrefab, pos, Quaternion.identity);
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }
}
