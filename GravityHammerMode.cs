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
    public class GravityHammerMode : SpellMode<SpellCastGravity>
    {
        private float rotation;
        private int partPicked = 6;
        private float distanceBetweenPart = 0.1f;
        private List<Rigidbody> hammerHeadPart;
        private List<int> nbHammerHeadPart = new List<int> { 1, 2, 3, 8 };
        private List<int> nbHammerBodyPart = new List<int> { 4, 5, 6, 7, 10 };
        private List<int> nbHammerHandlePart = new List<int> { 9, 11, 12, 13, 14 };
        private Vector3 originOfHead;
        private Vector3 originOfBody;
        private Vector3 originOfHandle;
        private float handleRadius = 0.1f;
        public float radiusOfImpact = 3f;
        private float cooldownEffect = 0.75f;
        private float lastTimeEffect;
        public string imbueHitGroundEffectId;
        private EffectData imbueHitGroundEffectData;
        private EffectInstance effectInstance;
        private bool effectIsPlaying = false;



        public override void OnItemLoaded(Item item) { base.OnItemLoaded(item); }
        public override int TargetPartNum() => partPicked;

        public override Vector3 Center()
        {
            return Hand().transform.position + ForwardDir() * 0.1f;
        }

        public override void Enter(Shatterblade.Shatterblade sword)
        {
            base.Enter(sword);
            hammerHeadPart = new List<Rigidbody>();
            int i = 1;
            foreach (Rigidbody rb in jointParts)
            {
                if (nbHammerHeadPart.Contains(i))
                {
                    hammerHeadPart.Add(rb);
                }
                i++;
            }
            imbueHitGroundEffectData = Catalog.GetData<EffectData>(imbueHitGroundEffectId);
        }

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            originOfHead = Center() + UpDir() * 0.75f;
            originOfBody = Center() + UpDir() * 0.25f;
            originOfHandle = Center() + UpDir() * -0.25f;
            if (nbHammerHeadPart.Contains(index))
            {
                return originOfHead;
            }
            else if (nbHammerBodyPart.Contains(index))
            {

                if (nbHammerBodyPart.IndexOf(index) != 0)
                {
                    if (nbHammerBodyPart.IndexOf(index) % 2 == 0)
                    {
                        return originOfBody + UpDir() * distanceBetweenPart * (nbHammerBodyPart.IndexOf(index) / 2 + 1);
                    }
                    else
                    {
                        return originOfBody + UpDir() * (-distanceBetweenPart * (nbHammerBodyPart.IndexOf(index) / 2 + 1));
                    }
                }
                else
                {
                    return originOfBody;
                }
            }
            else
            {
                return originOfHandle + Quaternion.AngleAxis((float)(nbHammerHandlePart.IndexOf(index)) / (nbHammerHandlePart.Count()) * 360f + rotation, ForwardDir()) * UpDir() * handleRadius;
            }
        }

        public override Quaternion GetRot(int index, Rigidbody rb, BladePart part)
        {
            if (nbHammerBodyPart.Contains(index))
            {
                return Quaternion.LookRotation(SideDir(), rb.transform.position - Center());
            }
            else
            {
                return Quaternion.LookRotation(ForwardDir(), rb.transform.position - Center());
            }
        }

        public override void OnTriggerPressed()
        {
            base.OnTriggerPressed();
            foreach (Rigidbody rb in hammerHeadPart)
            {
                sword.rbMap[rb].item.mainCollisionHandler.OnCollisionStartEvent += Part_OnCollisionStartEvent;
            }
        }

        private void Part_OnCollisionStartEvent(CollisionInstance hit)
        {
            //Debug.Log($"Gravity Hammer : hit intensity  : {hit.intensity}");
            if (hit.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart ragdollPart && ragdollPart.ragdoll.creature != Player.local.creature && hit.intensity > 0.25f)
            {
                if (ragdollPart.ragdoll.creature.state == Creature.State.Alive && ragdollPart.ragdoll.creature.state != Creature.State.Destabilized)
                {
                    ragdollPart.ragdoll.creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
                foreach (Rigidbody rigidbody in ragdollPart.ragdoll.parts.Select(part => part.rb))
                {
                    rigidbody.AddForce(hit.impactVelocity / 2f, ForceMode.VelocityChange);
                }
            }
            // On terrain hit
            if (!hit.targetColliderGroup?.collisionHandler?.item && !hit.targetColliderGroup?.collisionHandler?.ragdollPart)
            {
                foreach(Item item in Snippet.ItemsInRadius(hit.contactPoint, radiusOfImpact).Where(item => !item.rb.isKinematic))
                {
                    item.rb.AddExplosionForce(Vector3.Distance(hit.contactPoint, item.transform.position) * hit.impactVelocity.magnitude * (item.rb.mass < 2f ? 2f : item.rb.mass) / 2f, hit.contactPoint, radiusOfImpact, 1f, ForceMode.Impulse);
                }
                foreach(Creature creature in Snippet.CreaturesInRadiusMinusPlayer(hit.contactPoint, radiusOfImpact))
                {
                    if(creature.state == Creature.State.Alive && creature.state != Creature.State.Destabilized)
                    {
                        creature.ragdoll.SetState(Ragdoll.State.Destabilized, true);
                    }
                    foreach(RagdollPart part in creature.ragdoll.parts)
                    {
                        part.rb.AddExplosionForce(Vector3.Distance(hit.contactPoint, part.ragdoll.creature.transform.position) * hit.impactVelocity.magnitude * 2f, hit.contactPoint, radiusOfImpact, 1f, ForceMode.Impulse);
                    }
                }
                float t = Mathf.InverseLerp(4f, 15f, hit.impactVelocity.magnitude);
                if (!effectIsPlaying)
                {
                    effectInstance = imbueHitGroundEffectData.Spawn(hit.contactPoint, Quaternion.LookRotation(-hit.contactNormal, hit.sourceColliderGroup.transform.up));
                    effectInstance.Play();
                    effectInstance.SetIntensity(t);
                    lastTimeEffect = Time.time;
                    effectIsPlaying = true;
                }
            }
        }

        public override void OnTriggerReleased()
        {
            base.OnTriggerReleased();
            foreach (Rigidbody rb in hammerHeadPart)
            {
                sword.rbMap[rb].item.mainCollisionHandler.OnCollisionStartEvent -= Part_OnCollisionStartEvent;
                //Debug.Log($"Gravity Hammer : Event destroyed : {hammerHeadPart.IndexOf(rb)}");
            }
        }

        public override void Update()
        {
            base.Update();
            rotation += Time.deltaTime * 80f;
            if((lastTimeEffect < Time.time - cooldownEffect) && effectIsPlaying)
            {
                effectIsPlaying = false;
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
            joint.massScale = 20f;
            base.JointModifier(joint, part);
        }

        public override void Exit()
        {
            base.Exit();
            hammerHeadPart.Clear();
        }

    }
}
