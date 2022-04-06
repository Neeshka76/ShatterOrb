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

namespace ShatterOrb
{
    public class WingMode : SpellMode<SpellCastLightning>
    {
        private int partPicked = 6;
        private List<int> nbMainWing = new List<int> { 1, 2, 3, 9, 10, 11, 15 };
        private List<int> nbOtherWing = new List<int> { 4, 5, 7, 8, 12, 13, 14 };
        private float spanOfWing = 1.0f;
        private float spanBetweenParts = 0.2f;
        private bool canFly = false;
        private bool flightMode = false;
        private Vector3 playerGravity;
        private float dragsOriValue;

        public RagdollHand OtherHand() => GetPart().item.mainHandler.otherHand;

        /// <returns>The other hand holding the trigger shard.</returns>
        public Vector3 CenterOtherHand() => OtherHand().transform.position + OtherHand().PointDir() * 0.2f;

        /// <returns>Direction up from the other player's hand (in the direction of their thumb)</returns>
        public virtual Vector3 UpDirOtherHand() => OtherHand().ThumbDir();

        /// <returns>Direction forwards from the other player's hand (as if they were pointing)</returns>
        public virtual Vector3 ForwardDirOtherHand() => OtherHand().PointDir();

        /// <returns>Direction to the side from the other player's hand (in the direction of their palm)</returns>
        public virtual Vector3 SideDirOtherHand() => OtherHand().PalmDir();


        public override void OnItemLoaded(Item item) { base.OnItemLoaded(item); }
        public override int TargetPartNum() => partPicked;

        public override void Enter(Shatterblade.Shatterblade sword)
        {
            base.Enter(sword);
            playerGravity = Physics.gravity;
            dragsOriValue = Player.local.creature.locomotion.rb.drag;
        }

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            if (nbMainWing.Contains(index))
            {
                if (nbMainWing.IndexOf(index) == 0)
                {
                    return Center() + ForwardDir() * spanOfWing * (1f / 3f);
                }
                else if (nbMainWing.IndexOf(index) == 1 || nbMainWing.IndexOf(index) == 2)
                {
                    if (nbMainWing.IndexOf(index) == 1)
                    {
                        return Center() + ForwardDir() * spanOfWing * (2f / 3f);
                    }
                    else
                    {
                        return Center() + ForwardDir() * spanOfWing * (2f / 3f) + UpDir() * spanBetweenParts;
                    }
                }
                else
                {
                    if (nbMainWing.IndexOf(index) == 3)
                    {
                        return Center() + ForwardDir() * spanOfWing * (3f / 3f);
                    }
                    else if (nbMainWing.IndexOf(index) == 4)
                    {
                        return Center() + ForwardDir() * spanOfWing * (3f / 3f) + UpDir() * spanBetweenParts;
                    }
                    else if (nbMainWing.IndexOf(index) == 5)
                    {
                        return Center() + ForwardDir() * spanOfWing * (3f / 3f) + UpDir() * (-spanBetweenParts);
                    }
                    else if (nbMainWing.IndexOf(index) == 6)
                    {
                        return Center() + ForwardDir() * spanOfWing * (3f / 3f) + UpDir() * spanBetweenParts * 2f;
                    }
                    else
                    {
                        return Center() + ForwardDir() * spanOfWing * (3f / 3f) + UpDir() * (-spanBetweenParts) * 2f;
                    }
                }
            }
            else
            {
                if (nbOtherWing.IndexOf(index) == 0)
                {
                    return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (1f / 3f);
                }
                else if (nbOtherWing.IndexOf(index) == 1 || nbOtherWing.IndexOf(index) == 2)
                {
                    if (nbOtherWing.IndexOf(index) == 1)
                    {
                        return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (2f / 3f);
                    }
                    else
                    {
                        return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (2f / 3f) + UpDirOtherHand() * spanBetweenParts;
                    }
                }
                else
                {
                    if (nbOtherWing.IndexOf(index) == 3)
                    {
                        return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (3f / 3f);
                    }
                    else if (nbOtherWing.IndexOf(index) == 4)
                    {
                        return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (3f / 3f) + UpDirOtherHand() * spanBetweenParts;
                    }
                    else if (nbOtherWing.IndexOf(index) == 5)
                    {
                        return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (3f / 3f) + UpDirOtherHand() * (-spanBetweenParts);
                    }
                    else if (nbOtherWing.IndexOf(index) == 6)
                    {
                        return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (3f / 3f) + UpDirOtherHand() * spanBetweenParts * 2f;
                    }
                    else
                    {
                        return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (3f / 3f) + UpDirOtherHand() * (-spanBetweenParts) * 2f;
                    }
                }
            }
        }

        public override Quaternion GetRot(int index, Rigidbody rb, BladePart part)
        {
            if (nbMainWing.Contains(index))
            {
                return Quaternion.LookRotation(UpDir(), rb.transform.position - Center());
            }
            else
            {
                return Quaternion.LookRotation(UpDirOtherHand(), rb.transform.position - CenterOtherHand());
            }
        }

        public override void Update()
        {
            base.Update();
            if (flightMode)
            {
                int i = 1;
                foreach (BladePart parts in sword.parts)
                {
                    canFly = parts.item.rb.velocity.sqrMagnitude > 50f
                        && Vector3.SignedAngle(Player.local.creature.transform.forward, Vector3.Cross(parts.item.rb.velocity, Player.local.creature.transform.right), Player.local.transform.forward) < 90f
                        && (nbMainWing.Contains(i) && Hand().Velocity().sqrMagnitude > 10f || nbOtherWing.Contains(i) && OtherHand().Velocity().sqrMagnitude > 10f);
                    if (canFly)
                    {
                        if (Player.local.creature.locomotion.isGrounded)
                        {
                            Player.local.locomotion.rb.AddForce(-parts.item.rb.velocity * 2f, ForceMode.Impulse);
                        }
                        else
                        {
                            Player.local.locomotion.rb.AddForce(-parts.item.rb.velocity * 2f / 2f, ForceMode.Impulse);
                        }
                    }
                    i++;
                }
                Debug.Log($"Wing Mode : Hands : { Vector3.Dot(ForwardDir(), ForwardDirOtherHand())}");
                if (Vector3.Dot(ForwardDir(), ForwardDirOtherHand()) > -1f && Vector3.Dot(ForwardDir(), ForwardDirOtherHand()) < -0.5f && Vector3.Distance(Hand().transform.position, OtherHand().transform.position) > .75f)
                {
                    Player.local.locomotion.rb.drag = 5f;
                }
                else
                {
                    if (Player.local.locomotion.rb.drag != dragsOriValue)
                    {
                        Player.local.locomotion.rb.drag = dragsOriValue;
                    }

                }
            }
            else
            {
                if (Player.local.locomotion.rb.drag != dragsOriValue)
                {
                    Player.local.locomotion.rb.drag = dragsOriValue;
                }
            }
        }

        public override void JointModifier(ConfigurableJoint joint, BladePart part)
        {
            JointDrive posDrive = new JointDrive
            {
                positionSpring = 10000,
                positionDamper = 10,
                maximumForce = sword.module.jointMaxForce
            };
            JointDrive rotDrive = new JointDrive
            {
                positionSpring = 10000,
                positionDamper = 10,
                maximumForce = sword.module.jointMaxForce
            };
            joint.xDrive = posDrive;
            joint.yDrive = posDrive;
            joint.zDrive = posDrive;
            joint.angularXDrive = rotDrive;
            joint.angularYZDrive = rotDrive;
            joint.massScale = 20f;
            base.JointModifier(joint, part);
        }

        public override void OnTriggerPressed()
        {
            base.OnTriggerPressed();
            flightMode = true;
        }

        public override void OnTriggerReleased()
        {
            base.OnTriggerReleased();
            flightMode = false;
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
