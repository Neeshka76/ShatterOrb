using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;
using Shatterblade;
using Shatterblade.Modes;
using SnippetCode;
using DaggerBending;

namespace ShatterOrb
{
    public class DaggerMode : SpellMode<SpellDagger>
    {
        private int partPicked = 6;
        private bool reverseMain = false;
        private bool reverseOther = false;
        private float spanOfDagger = 0.1f;

        private bool wasOtherButtonPressed;
        private bool wasOtherTriggerPressed;
        /// <summary>The time at which the other button was last pressed.</summary>
        protected float lastOtherButtonPress;

        /// <summary>The time at which the other trigger was last pressed.</summary>
        protected float lastOtherTriggerPress;

        /// <summary>The time at which the other button was last released.</summary>
        protected float lastOtherButtonReleased;

        /// <summary>The time at which the other trigger was last released.</summary>
        protected float lastOtherTriggerReleased;




        private List<int> nbMainDagger = new List<int> { 12, 6, 5, 14, 1, 9, 4 };

        private List<int> nbMainHandleDagger = new List<int> { 12, 6 };
        private List<int> nbMainSwordDagger = new List<int> { 5, 14, 1 };
        private List<int> nbMainGuardDagger = new List<int> { 9, 4 };

        private List<int> nbOtherDagger = new List<int> { 10, 8, 7, 3, 2, 11, 13 };

        private List<int> nbOtherHandleDagger = new List<int> { 10, 8 };
        private List<int> nbOtherSwordDagger = new List<int> { 7, 3, 2 };
        private List<int> nbOtherGuardDagger = new List<int> { 11, 13 };


        public RagdollHand OtherHand() => GetPart().item.mainHandler.otherHand;

        /// <returns>The other hand holding the trigger shard.</returns>
        public Vector3 CenterOtherHand() => OtherHand().transform.position + OtherHand().PointDir() * 0.25f;

        /// <returns>Direction up from the other player's hand (in the direction of their thumb)</returns>
        public virtual Vector3 UpDirOtherHand() => OtherHand().ThumbDir();

        /// <returns>Direction forwards from the other player's hand (as if they were pointing)</returns>
        public virtual Vector3 ForwardDirOtherHand() => OtherHand().PointDir();

        /// <returns>Direction to the side from the other player's hand (in the direction of their palm)</returns>
        public virtual Vector3 SideDirOtherHand() => OtherHand().PalmDir();

        /// <returns>True if the player is pressing the alt use button on their controller.</returns>
        public bool IsOtherButtonPressed() => OtherHand().playerHand.controlHand.alternateUsePressed;

        /// <returns>True if the player is pressing the trigger on their controller.</returns>
        public bool IsOtherTriggerPressed() => OtherHand().playerHand.controlHand.usePressed && Time.time - lastOtherTriggerReleased > Cooldown();
        public override Vector3 Center() => Hand().transform.position + Hand().PointDir() * 0.25f;

        /// <summary>
        /// Called when the other button is pressed.
        /// </summary>
        public virtual void OnOtherButtonPressed() => lastOtherButtonPress = Time.time;

        /// <summary>
        /// Called once per frame while the other button is held.
        /// </summary>
        public virtual void OnOtherButtonHeld() { }

        /// <summary>
        /// Called once per frame while the other button is not held.
        /// </summary>
        public virtual void OnOtherButtonNotHeld() { }

        /// <summary>
        /// Called when the other button is released.
        /// </summary>
        public virtual void OnOtherButtonReleased() => lastOtherButtonReleased = Time.time;

        /// <summary>
        /// Called when the other trigger is pressed.
        /// </summary>
        public virtual void OnOtherTriggerPressed()
        {
            lastOtherTriggerPress = Time.time;
            reverseOther ^= true;
        }

        /// <summary>
        /// Called once per frame while the other trigger is held.
        /// </summary>
        public virtual void OnOtherTriggerHeld() { }

        /// <summary>
        /// Called once per frame while the other trigger is not held.
        /// </summary>
        public virtual void OnOtherTriggerNotHeld() { }

        /// <summary>
        /// Called when the other trigger is released.
        /// </summary>
        public virtual void OnOtherTriggerReleased() => lastOtherTriggerReleased = Time.time;

        public override void OnItemLoaded(Item item) { base.OnItemLoaded(item); }
        public override int TargetPartNum() => partPicked;

        public override void Enter(Shatterblade.Shatterblade sword)
        {
            base.Enter(sword);
        }

        private void CheckOtherInputs()
        {
            if (IsOtherTriggerPressed())
            {
                if (!wasOtherTriggerPressed)
                {
                    wasOtherTriggerPressed = true;
                    OnOtherTriggerPressed();
                }
                OnOtherTriggerHeld();
            }
            else
            {
                if (wasOtherTriggerPressed)
                {
                    wasOtherTriggerPressed = false;
                    OnOtherTriggerReleased();
                }
                OnOtherTriggerNotHeld();
            }
            if (IsOtherButtonPressed())
            {
                if (!wasOtherButtonPressed)
                {
                    wasOtherButtonPressed = true;
                    OnOtherButtonPressed();
                }
                OnOtherButtonHeld();
            }
            else
            {
                if (wasOtherButtonPressed)
                {
                    wasOtherButtonPressed = false;
                    OnOtherButtonReleased();
                }
                OnOtherButtonNotHeld();
            }
        }

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            Vector3 pos;
            if (nbMainDagger.Contains(index))
            {
                if (nbMainSwordDagger.Contains(index))
                {
                    switch (nbMainSwordDagger.IndexOf(index))
                    {
                        case 0:
                            pos = Center() + (reverseMain ? -1f : 1f) * UpDir() * spanOfDagger * 3f;
                            break;
                        case 1:
                            pos = Center() + (reverseMain ? -1f : 1f) * UpDir() * spanOfDagger * 4f;
                            break;
                        case 2:
                            pos = Center() + (reverseMain ? -1f : 1f) * UpDir() * spanOfDagger * 5f;
                            break;
                        default:
                            pos = Center();
                            break;
                    }
                }
                else if (nbMainGuardDagger.Contains(index))
                {
                    switch (nbMainGuardDagger.IndexOf(index))
                    {
                        case 0:
                            pos = Center() + (reverseMain ? -1f : 1f) * UpDir() * spanOfDagger * 2f + SideDir() * (reverseMain ? -1f : 1f) * spanOfDagger;
                            break;
                        case 1:
                            pos = Center() + (reverseMain ? -1f : 1f) * UpDir() * spanOfDagger * 2f + SideDir() * (reverseMain ? 1f : -1f) * spanOfDagger;
                            break;
                        default:
                            pos = Center();
                            break;
                    }
                }
                else
                {
                    switch (nbMainHandleDagger.IndexOf(index))
                    {
                        case 0:
                            pos = Center() + (reverseMain ? -1f : 1f) * UpDir() * spanOfDagger;
                            break;
                        case 1:
                            pos = Center() + (reverseMain ? -1f : 1f) * UpDir() * spanOfDagger * 2f;
                            break;
                        default:
                            pos = Center();
                            break;
                    }
                }
            }
            else
            {
                if (nbOtherSwordDagger.Contains(index))
                {
                    switch (nbOtherSwordDagger.IndexOf(index))
                    {
                        case 0:
                            pos = CenterOtherHand() + (reverseOther ? -1f : 1f) * UpDirOtherHand() * spanOfDagger * 3f;
                            break;
                        case 1:
                            pos = CenterOtherHand() + (reverseOther ? -1f : 1f) * UpDirOtherHand() * spanOfDagger * 4f;
                            break;
                        case 2:
                            pos = CenterOtherHand() + (reverseOther ? -1f : 1f) * UpDirOtherHand() * spanOfDagger * 5f;
                            break;
                        default:
                            pos = CenterOtherHand();
                            break;
                    }
                }
                else if (nbOtherGuardDagger.Contains(index))
                {
                    switch (nbOtherGuardDagger.IndexOf(index))
                    {
                        case 0:
                            pos = CenterOtherHand() + (reverseOther ? -1f : 1f) * UpDirOtherHand() * spanOfDagger * 2f + SideDirOtherHand() * (reverseOther ? -1f : 1f) * spanOfDagger;
                            break;
                        case 1:
                            pos = CenterOtherHand() + (reverseOther ? -1f : 1f) * UpDirOtherHand() * spanOfDagger * 2f + SideDirOtherHand() * (reverseOther ? 1f : -1f) * spanOfDagger;
                            break;
                        default:
                            pos = CenterOtherHand();
                            break;
                    }
                }
                else
                {
                    switch (nbOtherHandleDagger.IndexOf(index))
                    {
                        case 0:
                            pos = CenterOtherHand() + (reverseOther ? -1f : 1f) * UpDirOtherHand() * spanOfDagger;
                            break;
                        case 1:
                            pos = CenterOtherHand() + (reverseOther ? -1f : 1f) * UpDirOtherHand() * spanOfDagger * 2f;
                            break;
                        default:
                            pos = CenterOtherHand();
                            break;
                    }
                }
            }

            return pos;
        }

        public override string GetUseAnnotation() => "Press trigger of the shard to reverse the dagger; \n Press trigger of the other hand to reverse the other dagger";
        public override bool GetUseAnnotationShown() => true;

        public override void OnTriggerPressed()
        {
            base.OnTriggerPressed();
            reverseMain ^= true;
        }

        public override void Update()
        {
            base.Update();
            CheckOtherInputs();
        }

        public override Quaternion GetRot(int index, Rigidbody rb, BladePart part)
        {
            if (nbMainDagger.Contains(index))
            {
                return Quaternion.LookRotation((reverseMain ? -1f : 1f) * UpDir(), rb.transform.position - Center());
            }
            else
            {
                return Quaternion.LookRotation((reverseOther ? -1f : 1f) * UpDirOtherHand(), rb.transform.position - CenterOtherHand());
            }
        }

        public override void JointModifier(ConfigurableJoint joint, BladePart part)
        {
            JointDrive posDrive = new JointDrive
            {
                positionSpring = 2000,
                positionDamper = 40,
                maximumForce = sword.module.jointMaxForce
            };
            JointDrive rotDrive = new JointDrive
            {
                positionSpring = 1000,
                positionDamper = 40,
                maximumForce = sword.module.jointMaxForce
            };
            SoftJointLimit softJointLimit = new SoftJointLimit
            {
                limit = 0.05f,
                bounciness = 0f,
                contactDistance = 0f
            };
            joint.linearLimit = softJointLimit;
            joint.xDrive = posDrive;
            joint.yDrive = posDrive;
            joint.zDrive = posDrive;
            joint.angularXDrive = rotDrive;
            joint.angularYZDrive = rotDrive;
            joint.massScale = 20f;
            base.JointModifier(joint, part);
        }

        public override void Exit()
        {
            base.Exit();
            reverseMain = false;
            reverseOther = false;
        }

    }
}
