using System;
using System.Collections;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int currentHealth = 3;
        [SerializeField] private float hitAnimationTime = 1.45f;
        [SerializeField] private float blinkTime = 1.3f;
        [SerializeField] private float blinkStepTime = .1f;
        [SerializeField] private bool isImmortal;
        [SerializeField] private Renderer[] blinkRenderers;
        
        private bool _isDead;
        public bool IsDead => _isDead;

        private PlayerController _controller;
        public PlayerController Controller => _controller ? _controller : (_controller = GetComponent<PlayerController>());
        
        private void Start()
        {
            UIManager.Instance.UpdateHealth(currentHealth);
        }

        public void TakeDamage()
        {
            if (_isDead || isImmortal)
                return;

            currentHealth--;
            UIManager.Instance.UpdateHealth(currentHealth);
            Controller.isMove = false;
            Controller.Animator.Play("Hit");
            Controller.playerCamera.DOShakePosition(hitAnimationTime / 2, 0.5f, 20, 100)
                .OnComplete(() => {Controller.isMove = true;});
            Controller.playerCamera.DOFieldOfView(65, (hitAnimationTime * 7f / 10)).OnComplete(() => {
                Controller.playerCamera.DOFieldOfView(70, (hitAnimationTime * 3f / 10));
            });

            if (currentHealth <= 0)
                Die();
            else
            
                StartCoroutine(HitReaction());
        }
        
        private IEnumerator HitReaction()
        {
            int step = (int)(blinkTime / blinkStepTime);
            isImmortal = true;
            
            for (int i = 0; i < step; i++)
            {
                yield return new WaitForSeconds(blinkStepTime);
                foreach (var renderer in blinkRenderers)
                    renderer.enabled = !renderer.enabled;
            }
            
            foreach (var renderer in blinkRenderers)
                renderer.enabled = true;

            isImmortal = false;
            Controller.isMove = true;
        }

        private void Die()
        {
            _isDead = true;
            Controller.Animator.Play("Die");
            Controller.playerCamera.transform.DOMoveZ(Controller.playerCamera.transform.position.z - 5, 1.5f).SetEase(Ease.InOutSine);
        }
    }
}