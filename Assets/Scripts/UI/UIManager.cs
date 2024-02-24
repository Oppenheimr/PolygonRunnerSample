using System;
using System.Collections;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityUtils.BaseClasses;

namespace UI
{
    public class UIManager : SingletonBehavior<UIManager>
    {
        [SerializeField] private TextMeshProUGUI health;
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private TextMeshProUGUI gameOverScore;
        [SerializeField] private CanvasGroup startPanel;
        [SerializeField] private CanvasGroup gamePanel;
        [SerializeField] private CanvasGroup gameOverPanel;

        private bool _scoreIsHigh;

        private void Start()
        {
            startPanel.gameObject.SetActive(true);
            startPanel.alpha = 1;

            gamePanel.gameObject.SetActive(false);
            gamePanel.alpha = 0;

            gameOverPanel.gameObject.SetActive(false);
            gameOverPanel.alpha = 0;

            StartCoroutine(ScoreCounter());
        }

        private IEnumerator ScoreCounter()
        {
            while (true)
            {
                yield return new WaitForSeconds(.1f);
                if (!GameManager.Instance.IsPlay)
                    continue;

                GameManager.Instance.Score++;

                if (!_scoreIsHigh && ScoreRecorder.IsNewHighScore(GameManager.Instance.Score))
                {
                    score.color = Color.red;
                    _scoreIsHigh = true;
                }
                UpdateScore();
            }
        }

        public void CollectCoin()
        {
            GameManager.Instance.Score += 10;
            UpdateScore();
            score.DOColor(Color.yellow, .2f).OnComplete(() => {
                score.DOColor(_scoreIsHigh ? Color.red : Color.white, .1f);
            });
        }

        private void UpdateScore() => score.text = _scoreIsHigh ? 
            $"New High Score : {GameManager.Instance.Score}" 
            : $"Score : {GameManager.Instance.Score}";
        
        public void UpdateHealth(int health)
        {
            if (health == 0)
            {
                gamePanel.DOFade(0, 1).OnComplete(() => { gamePanel.gameObject.SetActive(false); });

                gameOverPanel.gameObject.SetActive(true);
                gameOverPanel.DOFade(1, 4);

                if (ScoreRecorder.TrySetNewHighScore(GameManager.Instance.Score))
                {
                    gameOverScore.text = $"New High Score : {GameManager.Instance.Score}";
                    gameOverScore.color = Color.red;
                }
                else
                    gameOverScore.text = $"Score : {GameManager.Instance.Score}";

                return;
            }

            this.health.text = $"Health : {health}";
        }

        public void Play()
        {
            startPanel.DOFade(0, 1).OnComplete(() => { startPanel.gameObject.SetActive(false); });

            gamePanel.gameObject.SetActive(true);
            gamePanel.DOFade(1, 1);
            GameManager.Instance.IsPlay = true;
            GameManager.Instance.Player.Run();
        }

        /// <summary>
        /// This method will be called when the player dies.
        /// Reload the scene.
        /// </summary>
        public void Replay()
        {
            DOTween.Clear(true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
            Application.Quit();
        }

    }
}