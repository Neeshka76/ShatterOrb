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
        private List<int> nbHammerHeadPart = new List<int>{ 1, 2, 3, 8 };
        private List<int> nbHammerBodyPart = new List<int>{ 4, 5, 7, 9, 11};
        private List<int> nbHammerHandlePart = new List<int>{ 10, 12, 13, 14, 15 };
        private Vector3 originOfHead;
        private Vector3 originOfBody;
        private Vector3 originOfHandle;
        private float handleRadius = 0.1f;

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
            foreach(Rigidbody rb in jointParts)
            {
                if (nbHammerHeadPart.Contains(i))
                {
                    hammerHeadPart.Add(rb);
                }
                i++;
            }
        }

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            originOfHead = Center() + UpDir() * 0.75f;
            originOfBody = Center() + UpDir() * 0.25f;
            originOfHandle = Center() +  UpDir() * -0.25f;
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
                if(nbHammerHandlePart.IndexOf(index) == 1)
                {
                    //Debug.Log($"Gravity Hammer : Value of rotation : {rotation}");
                }
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
                Debug.Log($"Gravity Hammer : Event created : {hammerHeadPart.IndexOf(rb)}");
            }
        }

        private void Part_OnCollisionStartEvent(CollisionInstance hit)
        {
            Debug.Log($"Gravity Hammer : hit intensity  : {hit.intensity}");
            if (hit.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart ragdollPart && ragdollPart.ragdoll.creature != Player.local.creature && hit.intensity > 0.25f)
            {
                if (ragdollPart.ragdoll.creature.state == Creature.State.Alive && ragdollPart.ragdoll.creature.state != Creature.State.Destabilized)
                {
                    ragdollPart.ragdoll.creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
                foreach (Rigidbody rigidbody in ragdollPart.ragdoll.parts.Select(part => part.rb))
                {
                    rigidbody.AddForce(hit.impactVelocity.normalized * 5f, ForceMode.VelocityChange);
                }
            }
            // Finish Explosive shockwave !
            // On terrain hit
            if (!hit.targetColliderGroup?.collisionHandler?.item && !hit.targetColliderGroup?.collisionHandler?.ragdollPart)
            {

            }
        }

        public override void OnTriggerReleased()
        {
            base.OnTriggerReleased();
            foreach(Rigidbody rb in hammerHeadPart)
            {
                sword.rbMap[rb].item.mainCollisionHandler.OnCollisionStartEvent -= Part_OnCollisionStartEvent;
                Debug.Log($"Gravity Hammer : Event destroyed : {hammerHeadPart.IndexOf(rb)}");
            }
        }

        public override void Update()
        {
            base.Update();
            rotation += Time.deltaTime * 80f;
        }

        public override void Exit()
        {
            base.Exit();
            hammerHeadPart.Clear();
        }

    }
}
