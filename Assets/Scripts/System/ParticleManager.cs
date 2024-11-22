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
    [SerializeField] private GameObject mergeParticle;
    [SerializeField] private GameObject mergePowerParticle;
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private GameObject mergeTextPrefab;
    
    private Canvas mainCanvas => GameManager.Instance.mainCanvas;
    
    public void HealParticle(Vector3 pos)
    {
        Instantiate(healParticlePrefab, pos, Quaternion.identity);
    }
    
    public void HealParticleToPlayer()
    {
        var pos = new Vector3(-5.7f, 3.1f, 0);
        Instantiate(healParticlePrefab, pos, Quaternion.identity);
    }
    
    public void MergeParticle(Vector3 pos)
    {
        Instantiate(mergeParticle, pos, Quaternion.identity);
    }
    
    public void MergePowerParticle(Vector3 pos, Color color)
    {
        var mpp = Instantiate(mergePowerParticle, pos, Quaternion.identity).GetComponent<MergePowerParticle>();
        mpp.MoveTo(color);
    }
    
    public void MergeText(int value, Vector3 pos, Color color = default)
    {
        var mergeText = Instantiate(mergeTextPrefab, pos, Quaternion.identity, mainCanvas.transform);
        if (color == default) color = Color.white;
        mergeText.GetComponent<MergeText>().SetUp(value, color);
    }
    
    public void DamageText(int value, float xPos)
    {
        var damageText = Instantiate(damageTextPrefab, mainCanvas.transform);
        damageText.GetComponent<DamageText>().SetUp(value, xPos);
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }
}
