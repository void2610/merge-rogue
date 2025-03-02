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
    
    [Header("依存オブジェクト")]
    [SerializeField] private Transform textContainer;
    [Header("パーティクル")]
    [SerializeField] private GameObject healParticlePrefab;
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private GameObject allHitParticle;
    [SerializeField] private GameObject mergeParticle;
    [SerializeField] private GameObject mergePowerParticle;
    [SerializeField] private GameObject bombFireParticle;
    [SerializeField] private GameObject thunderParticle;
    [Header("テキスト")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private GameObject mergeTextPrefab;
    [SerializeField] private GameObject wavyTextPrefab;
    
    public void HealParticle(Vector3 pos) => Instantiate(healParticlePrefab, pos, Quaternion.identity);
    
    public void HealParticleToPlayer()
    {
        var pos = new Vector3(-5.7f, 3.1f, 0);
        Instantiate(healParticlePrefab, pos, Quaternion.identity);
    }
    
    public void HitParticle(Vector3 pos) => Instantiate(hitParticle, pos, Quaternion.identity);
    
    public void AllHitParticle(Vector3 pos) => Instantiate(allHitParticle, pos, Quaternion.identity);
    
    public void MergeParticle(Vector3 pos) => Instantiate(mergeParticle, pos, Quaternion.identity);
    
    public void ThunderParticle(Vector3 pos) => Instantiate(thunderParticle, pos, Quaternion.identity);
    
    public void MergePowerParticle(Vector3 pos, Color color)
    {
        var mpp = Instantiate(mergePowerParticle, pos, Quaternion.identity).GetComponent<MergePowerParticle>();
        mpp.MoveTo(color);
    }
    
    public GameObject GetBombFireParticle() => Instantiate(bombFireParticle);
    
    public void MergeText(int value, Vector3 pos, Color color = default)
    {
        var r = new Vector3(UnityEngine.Random.Range(-0.75f, 0.75f), UnityEngine.Random.Range(-0.75f, 0.75f), 0);
        var mergeText = Instantiate(mergeTextPrefab, pos + r, Quaternion.identity, textContainer);
        if (color == default) color = Color.white;
        mergeText.GetComponent<MergeText>().SetUp(value, color);
    }
    
    public void DamageText(int value, float xPos, Color color = default)
    {
        var damageText = Instantiate(damageTextPrefab, textContainer);
        damageText.GetComponent<DamageText>().SetUp(value, xPos, color);
    }
    
    public void WavyText(string text, Vector3 pos, Color color = default, float fontSize = 15f)
    {
        var wavyText = Instantiate(wavyTextPrefab, pos, Quaternion.identity, textContainer);
        wavyText.GetComponent<WavyText>().SetUp(text, color, fontSize);
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }
}
