using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using SonicRealms.Level.Areas;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// The special move performed when Sonic has the bubble shield. Dives down and then bounces back up.
    /// </summary>
    public class BubbleSpecial : DoubleJump
    {
        /// <summary>
        /// The velocity at which to dive, in units per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("The velocity at which to dive, in units per second.")]
        public Vector2 DiveVelocity;

        /// <summary>
        /// The velocity at which to bounce off the surface, in units per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("The speed at which to bounce off the surface, in units per second.")]
        public float BounceSpeed;

        /// <summary>
        /// The velocity of the bounce after releasing the jump key, in units per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("The velocity of the bounce after releasing the jump key, in units per second.")]
        public float BounceReleaseSpeed;

        /// <summary>
        /// When underwater, the velocity at which to bounce off the surface, in units per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("When underwater, the velocity at which to bounce off the surface, in units per second.")]
        public float UnderwaterBounceSpeed;

        /// <summary>
        /// When underwater, the velocity of the bounce after releasing the jump key, in units per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("When underwater, the velocity at which to bounce off the surface, in units per second.")]
        public float UnderwaterBounceReleaseSpeed;

        /// <summary>
        /// Name of an Animator trigger to set when the controller bounces.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator trigger to set when the controller bounces.")]
        public string BounceTrigger;
        protected int BounceTriggerHash;

        /// <summary>
        /// An audio clip to play when the dive occurs.
        /// </summary>
        [SoundFoldout]
        [Tooltip("An audio clip to play when the dive occurs.")]
        public AudioClip DiveSound;

        /// <summary>
        /// Audio source to replace with the bounce sound when it occurs.
        /// </summary>
        private AudioSource _diveAudioSource;

        /// <summary>
        /// An audio clip to play when the bounce occurs.
        /// </summary>
        [SoundFoldout]
        [Tooltip("An audio clip to play when the bounce occurs.")]
        public AudioClip BounceSound;

        /// <summary>
        /// Whether the controller is currently bouncing.
        /// </summary>
        public bool Bouncing;

        public override void Reset()
        {
            base.Reset();
            DiveVelocity = new Vector2(0.0f, -4.8f);
            BounceSpeed = 4.5f;
            BounceReleaseSpeed = 2.4f;

            // These values are not taken from the disassembly and were not available in the guide -
            // they were guesstimated. If you have more accurate values please let me know!
            UnderwaterBounceSpeed = 2.4f;
            UnderwaterBounceReleaseSpeed = 1.2f;

            BounceTrigger = "";
        }

        public override void OnManagerAdd()
        {
            base.OnManagerAdd();

            if (Animator == null) return;
            BounceTriggerHash = string.IsNullOrEmpty(BounceTrigger) ? 0 : Animator.StringToHash(BounceTrigger);
        }

        public override void OnActiveEnter()
        {
            base.OnActiveEnter();
            Controller.RelativeVelocity = DiveVelocity;

            if (DiveSound != null) _diveAudioSource = SoundManager.Instance.PlayClipAtPoint(DiveSound, transform.position);

            // Listen for collisions - need to bounce back up when we collide with the ground
            Controller.OnCollide.AddListener(OnCollide);
        }

        public override void OnActiveUpdate()
        {
            base.OnActiveUpdate();
            if (!Bouncing) return;

            // End if the jump key is released mid-bounce
            if (!Input.GetButton(InputName))
            {
                End();

                // Set vertical speed to release speed based on whether underwater
                if (Controller.Inside<Water>())
                {
                    if (Controller.RelativeVelocity.y > UnderwaterBounceReleaseSpeed)
                        Controller.RelativeVelocity = new Vector2(Controller.RelativeVelocity.x,
                            UnderwaterBounceReleaseSpeed);
                }
                else
                {
                    if (Controller.RelativeVelocity.y > BounceReleaseSpeed)
                        Controller.RelativeVelocity = new Vector2(Controller.RelativeVelocity.x, BounceReleaseSpeed);
                }
               
            }
        }

        public override void OnActiveExit()
        {
            base.OnActiveExit();
            Bouncing = false;
        }

        public void OnCollide(TerrainCastHit hit)
        {
            // Bounce only if it's the controller's bottom colliding with the floor
            if ((hit.Side & ControllerSide.Bottom) == 0) return;

            // Normally we can't use a double jump again until we attach to the floor, so make
            // it available manually
            Used = false;

            // Set bounce velocity based on surface normal and jump input
            if (Input.GetButton(InputName))
            {
                Controller.Velocity = Vector2.Reflect(Controller.Velocity, hit.Hit.normal).normalized*
                                      (Controller.Inside<Water>() ? UnderwaterBounceSpeed : BounceSpeed);
            }
            else
            {
                Controller.Velocity = Vector2.Reflect(Controller.Velocity, hit.Hit.normal).normalized *
                                      (Controller.Inside<Water>() ? UnderwaterBounceReleaseSpeed : BounceReleaseSpeed);

                // If the jump input is no longer held down end the move immediately
                End();
            }

            // Prevent controller from landing on the surface it just hit
            Controller.IgnoreThisCollision();

            Controller.OnCollide.RemoveListener(OnCollide);

            if (BounceSound != null)
            {
                // Interrupt the diving sound
                if(_diveAudioSource.clip == DiveSound) _diveAudioSource.Stop();
                SoundManager.Instance.PlayClipAtPoint(BounceSound, transform.position);
            }

            Bouncing = true;
            if (Animator != null && BounceTriggerHash != 0)
                Animator.SetTrigger(BounceTriggerHash);
        }
    }
}
