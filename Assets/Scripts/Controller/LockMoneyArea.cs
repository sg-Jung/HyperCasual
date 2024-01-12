using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LockMoneyArea : MonoBehaviour
{
    [Header("UI & ParticleSystems")]
    public TMP_Text moneyText;
    public GameObject UI;
    public ParticleSystem partyParticle;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip partySound;
   
    [Header("Money & isLock")]
    public int needMoney;
    public bool isLock;

    public void InitLockMoneyArea(int money)
    {
        SetNeedMoney(money);
        UI.SetActive(true);
        isLock = true;
    }

    public void SetNeedMoney(int money)
    {
        needMoney = money;
        moneyText.text = needMoney.ToString();
    }

    private void CheckPlayerEnoughMoney(PlayerController player)
    {
        if (player.curMoney >= needMoney)
        {
            player.SetMoney(-needMoney);
            needMoney = 0;

            UnLockParty();
        }
    }

    private void UnLockParty()
    {
        partyParticle.Play();
        audioSource.PlayOneShot(partySound);
        UI.SetActive(false);
        isLock = false;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            CheckPlayerEnoughMoney(player);
        }
    }
    
}
