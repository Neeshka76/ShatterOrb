using Shatterblade;
using Shatterblade.Modes;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace ShatterOrb
{
    public class WingMode : SpellMode<SpellCastLightning>
    {
        private int partPicked = 6;
        private List<int> nbMainWing = new List<int> { 1, 2, 3, 8, 9, 10, 14 };
        private List<int> nbOtherWing = new List<int> { 4, 5, 6, 7, 11, 12, 13 };
        private float spanOfWing = 1.0f;
        private float spanBetweenParts = 0.2f;
        private bool canFly = false;
        private bool flightMode = false;
        private bool wingPushMode = false;
        private bool canPush = false;
        private List<Item> shardsList;
        public float angleOfPush = 45f;
        private bool gliding;

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
            shardsList = new List<Item>();
            foreach(BladePart part in sword.parts)
            {
                shardsList.Add(part.item);
            }
        }

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            Vector3 pos;
            if (nbMainWing.Contains(index))
            {
                switch (nbMainWing.IndexOf(index))
                {
                    case 0:
                        pos = Center() + ForwardDir() * spanOfWing * (1f / 3f);
                        break;
                    case 1:
                        pos = Center() + ForwardDir() * spanOfWing * (2f / 3f);
                        break;
                    case 2:
                        pos = Center() + ForwardDir() * spanOfWing * (2f / 3f) + UpDir() * spanBetweenParts;
                        break;
                    case 3:
                        pos = Center() + ForwardDir() * spanOfWing * (3f / 3f);
                        break;
                    case 4:
                        pos = Center() + ForwardDir() * spanOfWing * (3f / 3f) + UpDir() * spanBetweenParts;
                        break;
                    case 5:
                        pos = Center() + ForwardDir() * spanOfWing * (3f / 3f) + UpDir() * (-spanBetweenParts);
                        break;
                    case 6:
                        pos = Center() + ForwardDir() * spanOfWing * (3f / 3f) + UpDir() * spanBetweenParts * 2f;
                        break;
                    default:
                        pos = Center();
                        break;
                }
            }
            else
            {
                switch (nbOtherWing.IndexOf(index))
                {
                    case 0:
                        pos = CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (1f / 3f);
                        break;
                    case 1:
                        pos = CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (2f / 3f);
                        break;
                    case 2:
                        pos = CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (2f / 3f) + UpDirOtherHand() * spanBetweenParts;
                        break;
                    case 3:
                        pos = CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (3f / 3f);
                        break;
                    case 4:
                        pos = CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (3f / 3f) + UpDirOtherHand() * spanBetweenParts;
                        break;
                    case 5:
                        pos = CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (3f / 3f) + UpDirOtherHand() * (-spanBetweenParts);
                        break;
                    case 6:
                        pos = CenterOtherHand() + ForwardDirOtherHand() * spanOfWing * (3f / 3f) + UpDirOtherHand() * spanBetweenParts * 2f;
                        break;
                    default:
                        pos = CenterOtherHand();
                        break;
                }
            }
            return pos;
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
                gliding = Vector3.Dot(ForwardDir(), ForwardDirOtherHand()) > -1f && Vector3.Dot(ForwardDir(), ForwardDirOtherHand()) < -0.5f && Vector3.Distance(Hand().transform.position, OtherHand().transform.position) > 0.75f;
                if (gliding)
                {
                    Snippet.SlowDownFallCreature();
                }
            }
            if (wingPushMode)
            {
                int i = 1;
                foreach (BladePart parts in sword.parts)
                {
                    canPush = parts.item.rb.velocity.sqrMagnitude > 50f
                        && Vector3.SignedAngle(Player.local.creature.transform.forward, Vector3.Cross(parts.item.rb.velocity, Player.local.creature.transform.right), Player.local.transform.forward) < 90f
                        && (nbMainWing.Contains(i) && Hand().Velocity().sqrMagnitude > 10f || nbOtherWing.Contains(i) && OtherHand().Velocity().sqrMagnitude > 10f);
                    if (canPush)
                    {
                        Vector3 centralDirectionOfWings = (ForwardDir() + ForwardDirOtherHand()).normalized;
                        foreach (Creature creature in Snippet.CreaturesInConeRadius(Player.local.creature.transform.position, 5f, centralDirectionOfWings, angleOfPush, true, true))
                        {
                            if (creature.state == Creature.State.Alive && creature.state != Creature.State.Destabilized)
                            {
                                creature.ragdoll.SetState(Ragdoll.State.Destabilized, true);
                            }
                            foreach (RagdollPart part in creature.ragdoll.parts)
                            {
                                part.rb.AddForce(centralDirectionOfWings * parts.item.rb.velocity.magnitude / 3f, ForceMode.Impulse);
                            }
                        }
                        foreach (Item item in Snippet.ItemsInConeRadius(Player.local.creature.transform.position, 5f, centralDirectionOfWings, angleOfPush))
                        {
                            if (sword.item != item && !shardsList.Contains(item))
                            {
                                item.rb.AddForce(centralDirectionOfWings * parts.item.rb.velocity.magnitude * (item.rb.mass < 2f ? 2f : item.rb.mass) / 7f, ForceMode.Impulse);
                            }
                        }
                    }
                }
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
            joint.xDrive = posDrive;
            joint.yDrive = posDrive;
            joint.zDrive = posDrive;
            joint.angularXDrive = rotDrive;
            joint.angularYZDrive = rotDrive;
            joint.massScale = 30f;
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

        public override void OnButtonPressed()
        {
            base.OnButtonPressed();
            wingPushMode = true;
        }

        public override void OnButtonReleased()
        {
            base.OnButtonReleased();
            wingPushMode = false;
        }

        public override bool GetUseAnnotationShown() => true;
        public override bool GetAltUseAnnotationShown() => true;
        public override string GetUseAnnotation()
        {
            if (flightMode)
            {
                if (!Player.local.creature.locomotion.isGrounded)
                {
                    return "Flap the wings to fly";
                }
                else
                {
                    return "You can glide when extending the arms !";
                }
            }
            else
            {
                return "When airborne, maintain extend your arms on the side and align them to be able to glide !";
            }

        }
        public override string GetAltUseAnnotation()
        {
            if (wingPushMode)
            {
                return "Flap the wings toward you to push npcs";
            }
            else
            {
                return "Press the spell wheel button to allow be able to create a push wave";
            }
        }

        public override void Exit()
        {
            base.Exit();
            flightMode = false;
            wingPushMode = false;
        }
    }
}
