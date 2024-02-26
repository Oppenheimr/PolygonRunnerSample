using System;
using System.Collections;
using Core;
using DG.Tweening;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        public bool isMove = true;
        [SerializeField] private float speed = 15;
        [SerializeField] private float jump = .13f;
        [SerializeField] private float gravityScale = .5f;
        [SerializeField] private float horizontalMovementTime = .2f;
        [SerializeField] private Ease horizontalMovementEase = Ease.OutQuart;

        [Header("Animation")] [Space(5)]
        [SerializeField] private float slideAnimationTime = 1.45f;
        [SerializeField] private float slideSpeedMultiplier = 1.125f;

        [Header("Acceleration")] [Space(5)]
        [SerializeField] private float accelerationRefreshRate = 0.2f;
        [SerializeField] private float acceleration = 0.2f;
        
        [Header("References")] [Space(5)]
        public Camera playerCamera;

        private Vector3 _motion;

        //left run, right run, jump or slide
        private bool _inAnimation;
        private bool _isAir;
        private bool _isSlide;
        

        private Animator _animator;
        public Animator Animator => _animator ? _animator : (_animator = GetComponent<Animator>());

        private Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody ? _rigidbody : (_rigidbody = GetComponent<Rigidbody>());

        private CharacterController _character;
        public CharacterController Character => _character ? _character : (_character = GetComponent<CharacterController>());
        
        private PlayerHealth _health;
        
        public PlayerHealth Health => _health ? _health : (_health = GetComponent<PlayerHealth>());

        void Start()
        {
            Animator.Play("Idle");
            PlayerInput.Instance.onSwipeHorizontal.AddListener(HorizontalMove);
            PlayerInput.Instance.onSwipeVertical.AddListener(VerticalMove);
            StartCoroutine(Acceleration());
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.Instance.IsPlay || Health.IsDead || !isMove)
                return;
            
            float oldY = _motion.y;
            _motion = transform.forward * speed * Time.deltaTime;

            if (_isSlide)
                _motion *= slideSpeedMultiplier;

            _motion.y = oldY;

            if (_isAir && Character.isGrounded && _motion.y < 0)
            {
                _isAir = false;
                Animator.Play("Run");
            }
            else
            {
                _motion.y -= gravityScale * Time.deltaTime;
            }

            Character.Move(_motion);
            
            if (Input.GetKeyDown(KeyCode.T))
                Time.timeScale = Math.Abs(Time.timeScale - 1) < 0.1f ? 0.1f : 1;
        }

    #region Horizontal

        private void HorizontalMove(int movement)
        {
            if (!GameManager.Instance.IsPlay)
                return;
            
            if (Health.IsDead)
                return;
            
            bool isLeft = movement > 0;
            if (transform.position.x < -3 && !isLeft)
                return;
            else if (transform.position.x > 3 && isLeft)
                return;

            if (_inAnimation)
                return;
            _inAnimation = true;

            StartCoroutine(HorizontalCalculator(movement));
            var endValue = HorizontalMove(movement > 0);
            transform.DOMoveX(endValue, horizontalMovementTime)
                .SetEase(horizontalMovementEase).OnComplete(() => {
                    _inAnimation = false;
                    Animator.SetFloat("Horizontal", 0);
                    
                    var position = transform.position;
                    position = new Vector3(endValue, position.y, position.z);
                    transform.position = position;
                });
        }

        private IEnumerator HorizontalCalculator(float endValue)
        {
            float elapsedTime = 0f;
            float startValue = 0f;

            while (elapsedTime < horizontalMovementTime / 2)
            {
                float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / (horizontalMovementTime / 2));
                Animator.SetFloat("Horizontal", currentValue);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            elapsedTime = 0f;
            startValue = endValue;

            while (elapsedTime < horizontalMovementTime / 2)
            {
                float currentValue = Mathf.Lerp(startValue, 0f, elapsedTime / (horizontalMovementTime / 2));
                Animator.SetFloat("Horizontal", currentValue);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Hareket tamamlandığında değeri sıfıra eşitle
            Animator.SetFloat("Horizontal", 0f);
        }

        private float HorizontalMove(bool isLeft) => transform.position.x + (isLeft ? +3.5f : -3.5f);

    #endregion

    #region Vertical

        private void VerticalMove(int movement)
        {
            if (!GameManager.Instance.IsPlay)
                return;
            
            if (Health.IsDead)
                return;

            
            if (_inAnimation || !Character.isGrounded)
                return;

            //_inAnimation = true;
            if (movement > 0.2f)
            {
                Animator.SetTrigger("Jump");
                //CharacterController için zıplama hareketini yapar.
                _motion.y = jump;
                _isAir = true;
            }
            else if (movement < -0.2f && !_isSlide)
            {
                StartCoroutine(Slide());
            }
        }

        private IEnumerator Slide()
        {
            Character.height = 1;
            Character.center = new Vector3(0, 0.5f, 0);
            playerCamera.DOFieldOfView(85, (slideAnimationTime * 7f / 10)).OnComplete(() => {
                playerCamera.DOFieldOfView(70, (slideAnimationTime * 3f / 10));
            });
            _isSlide = true;
            Animator.Play("Slide");
            //DOTween.Sequence().SetDelay(slideAnimationTime).OnComplete(() => { _inAnimation = false; });
            yield return new WaitForSeconds(slideAnimationTime);
            _isSlide = false;
            _inAnimation = false;

            Character.height = 1.8f;
            Character.center = new Vector3(0, 0.9f, 0);
        }

    #endregion

        private IEnumerator Acceleration()
        {
            while (true)
            {
                yield return new WaitForSeconds(accelerationRefreshRate);
                if (!Health.IsDead && GameManager.Instance.IsPlay)
                    speed += acceleration;
            }
        }
        
        public void Run() => Animator.SetTrigger("OutIdle");
    }
}