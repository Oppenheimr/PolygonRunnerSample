using System.Collections;
using Core;
using UnityEngine;

namespace Level
{
    public class Plane : MonoBehaviour
    {
        [SerializeField] private float speed = 10;

        private void OnEnable()
        {
            StartCoroutine(SlowUpdate());
        }
        
        private void Update ()
        {
            if (GameManager.Instance.IsPlay)
                transform.Translate(-transform.forward * (speed * Time.deltaTime));
        }
        
        private IEnumerator SlowUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
                
                if (GameManager.Instance.Player.transform.position.z < transform.position.z + 10)
                    continue;
                
                gameObject.SetActive(false);
                LevelGenerator.Instance.SetupPlane();
                break;
            }
        }
        
        public void SetStartPosition(Vector3 position)
        {
            var newPosition = position;
            newPosition.z = position.z;
            transform.position = newPosition;
        }
    }
}