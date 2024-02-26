using System;
using DG.Tweening;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level
{
    public class HitTrigger : MonoBehaviour
    {
        private bool _inAnimation;

        private void OnTriggerEnter(Collider other)
        {
            if (_inAnimation)
                return;

            if (!other.CompareTag("Player"))
                return;

            if (!other.TryGetComponent(out PlayerHealth playerHealth))
                return;

            playerHealth.TakeDamage();
            ObstacleReaction();
        }

        private void ObstacleReaction()
        {
            _inAnimation = true;
            var childrenColliders = GetComponentsInChildren<Collider>();
            var parentColliders = GetComponentsInParent<Collider>();
            
            var obstacle = GetComponentInParent<Obstacle>();
            Transform parent = null;
            
            if (obstacle != null)
                parent = obstacle.transform;
            else
            {
                var plane = GetComponentInParent<Plane>();
                if (plane != null)
                    parent = plane.transform;
                else
                {
                    Debug.LogError("No parent found for the obstacle!");
                }
            }

            parent.DOShakeScale(0.2f, 0.5f, 50, 200).SetDelay(0.1f).OnComplete(() => {
                foreach (var collider in childrenColliders)
                    collider.enabled = false;

                foreach (var collider in parentColliders)
                    collider.enabled = false;
                
                if (obstacle != null)
                    obstacle.Close();
                else if (parent != null)
                    parent.gameObject.SetActive(false);
                
                _inAnimation = false;
            });
        }
    }
}