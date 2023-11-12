using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// これは、プレーヤーのコントロールを実施するためのメイン・クラスである。
    /// クラス＝設計図です。
    /// 様々なGameObjectにコンポーネントとして、追加してあげると、この設計図通りに動きます。
    /// </summary>
    public class PlayerController : KinematicObject
    {
        /// <summary>
        /// ジャンプの音源
        /// </summary>
        public AudioClip jumpAudio;

        /// <summary>
        /// 復活の音源
        /// </summary>
        public AudioClip respawnAudio;

        /// <summary>
        /// ダメージをくらったときの音源
        /// </summary>
        public AudioClip ouchAudio;

        /// <summary>
        /// 水平方向の最大移動速度
        /// </summary>
        public float maxSpeed = 10;
        /// <summary>
        ///ジャンプした時の加速音
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        /// <summary>
        /// プレイヤージャンプの状態
        /// 接地状態で初期化している
        /// </summary>
        public JumpState jumpState = JumpState.Grounded;

        /// <summary>
        /// ジャンプを止めるフラグ
        /// </summary>
        private bool stopJump;


        /// <summary>
        /// 2D用の当たり判定
        /// </summary>
        public Collider2D collider2d;

        /// <summary>
        /// 音源を鳴らす装置
        /// </summary>
        public AudioSource audioSource;

        /// <summary>
        /// HitPointなどの健康を管理するクラス
        /// </summary>
        public Health health;

        /// <summary>
        /// 操作できるかのフラグ
        /// </summary>
        public bool controlEnabled = true;

        /// <summary>
        /// ジャンプのフラグ
        /// </summary>
        bool jump;

        /// <summary>
        /// 移動量のベクトル
        /// </summary>
        Vector2 move;

        /// <summary>
        /// 絵を表示するクラス
        /// </summary>
        SpriteRenderer spriteRenderer;

        /// <summary>
        /// アニメーションをつかさどるクラス
        /// </summary>
        internal Animator animator;

        /// <summary>
        /// ゲームのプレイヤーが持っている設定集
        /// </summary>
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        /// <summary>
        /// プレイヤーの絵とほかの絵との重なりを調べるクラス
        /// </summary>
        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void Update()
        {
            //操作可能だったら
            if (controlEnabled)
            {
                //moveのx座標に左右キーの値をセットする
                //例えば←キー（Aきー）だったら、-1
                move.x = Input.GetAxis("Horizontal");

                //もし、プレイヤーが接地していて、かつ、ジャンプのボタン（Space)が押された時
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                //ジャンプのボタン(Spece)を離した時
                else if (Input.GetButtonUp("Jump"))
                {
                    //stopJump = true;
                    //Schedule<PlayerStopJump>().player = this;
                }
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    Debug.Log("Zキーが押されました");
                }
            }
            //そうじゃなかったら
            else
            {
                ///moveのx座標に0入れる
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}