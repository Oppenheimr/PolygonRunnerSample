using System;
using DG.Tweening;
using UI;
using UnityEngine;

namespace Level
{
    public class Coin : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (!other.TryGetComponent(out Player.PlayerController controller))
                return;

            UIManager.Instance.CollectCoin();

            transform.DOMoveY(transform.position.y + 1.8f, 0.2f).SetEase(Ease.InOutSine);
            
            transform.DOShakeScale(0.1f, 1.75f, 100, 100).OnComplete(() => {
                gameObject.SetActive(false);
            });
            
            transform.DOScale(Vector3.one/10, 0.2f);    
        }
    }
}