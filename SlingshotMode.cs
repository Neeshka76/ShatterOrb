using Shatterblade;
using Shatterblade.Modes;
using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace ShatterOrb
{
    public class SlingshotMode : SpellMode<SpellCastProjectile>
    {
        private int partPicked = 6;
        private List<int> nbHandleSlingshot = new List<int> { 1, 3, 4 };
        private List<int> nbLeftHandleSlingshot = new List<int> { 5, 6 };
        private List<int> nbRightHandleSlingshot = new List<int> { 7, 8 };
        private List<int> nbStringSlingshot = new List<int> { 9, 10, 12, 11, 13, 14 };
        private int nbProjectileSlingshot = 2;
        private Dictionary<Rigidbody, RagdollPart> shardToPart = new Dictionary<Rigidbody, RagdollPart>();
        private RagdollPart partTarget;
        private Creature creatureTarget;
        private ConfigurableJoint throwJoint;
        private Rigidbody emptyHandle;
        private float spanOfString = 0.05f;
        private float spanOfHandle = 0.1f;
        private bool targetMode = false;
        private bool bounceMode = false;
        private bool grabbedShard = false;
        private bool shardThrowed = false;
        private bool stopThrow = false;
        private bool justHit = true;
        private int stepTarget = 0;
        private int stepBounce = 0;
        private float initialDistance;
        private int nbMaxBounce = 4;
        private int nbBounce = 0;
        private bool disableColliders = false;

        public RagdollHand OtherHand() => GetPart().item.mainHandler.otherHand;

        public override void OnItemLoaded(Item item) { base.OnItemLoaded(item); }
        public override int TargetPartNum() => partPicked;

        public override void Enter(Shatterblade.Shatterblade sword)
        {
            base.Enter(sword);
            emptyHandle = new GameObject().AddComponent<Rigidbody>();
            emptyHandle.useGravity = false;
            emptyHandle.isKinematic = true;
            emptyHandle.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
            sword.GetPart(nbProjectileSlingshot).item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance hit)
        {
            if (shardThrowed)
            {
                if (hit.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart ragdollPart && ragdollPart.ragdoll.creature != Player.local.creature)
                {
                    OnRagdollHit(ragdollPart, hit);
                }
                else if (hit.targetColliderGroup?.collisionHandler?.item is Item item)
                {
                    OnItemHit(item, hit);
                }
                else
                {
                    OnOtherHit(hit);
                }
            }
        }

        public void OnRagdollHit(RagdollPart ragdollPart, CollisionInstance hit)
        {
            if (ragdollPart.ragdoll.creature != Player.local.creature)
            {
                if (targetMode && (ragdollPart.type == RagdollPart.Type.Neck || ragdollPart.type == RagdollPart.Type.Torso || ragdollPart.type == RagdollPart.Type.Head))
                {
                    ragdollPart.ragdoll.headPart.Slice();
                    ragdollPart.ragdoll.creature.Kill();
                    sword.GetPart(nbProjectileSlingshot).item.Depenetrate();
                    sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(Vector3.up * 5f + (partTarget.transform.position - sword.GetPart(nbProjectileSlingshot).item.transform.position).normalized * 2f, ForceMode.Impulse);
                    stepTarget = 2;
                }
                if (bounceMode)
                {
                    sword.GetPart(nbProjectileSlingshot).item.Depenetrate();
                    sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(Vector3.Reflect(hit.impactVelocity.normalized, hit.contactNormal) * 0.75f, ForceMode.VelocityChange);
                    stepBounce = 0;
                    nbBounce++;
                }
            }
            justHit = true;
        }

        public void OnItemHit(Item item, CollisionInstance hit)
        {
            if (bounceMode)
            {
                sword.GetPart(nbProjectileSlingshot).item.Depenetrate();
                sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(Vector3.Reflect(hit.impactVelocity.normalized, hit.contactNormal) * 0.75f, ForceMode.VelocityChange);
                stepBounce = 0;
                nbBounce++;
            }
            justHit = true;
        }
        public void OnOtherHit(CollisionInstance hit)
        {
            if (bounceMode)
            {
                sword.GetPart(nbProjectileSlingshot).item.Depenetrate();
                sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(Vector3.Reflect(hit.impactVelocity.normalized, hit.contactNormal) * 0.75f, ForceMode.VelocityChange);
                stepBounce = 0;
                nbBounce++;
            }
            justHit = true;
        }

        public override Vector3 Center() => Hand().transform.position + Hand().PointDir() * 0.1f;

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            Vector3 pos;
            if (nbHandleSlingshot.Contains(index))
            {
                switch (nbHandleSlingshot.IndexOf(index))
                {
                    case 0:
                        pos = Center() + UpDir() * spanOfHandle * 1f;
                        break;
                    case 1:
                        pos = Center() + UpDir() * spanOfHandle * 2f;
                        break;
                    case 2:
                        pos = Center() + UpDir() * spanOfHandle * 3f;
                        break;
                    default:
                        pos = Center();
                        break;
                }
            }
            else if (nbLeftHandleSlingshot.Contains(index))
            {
                switch (nbLeftHandleSlingshot.IndexOf(index))
                {
                    case 0:
                        pos = Center() + UpDir() * spanOfHandle * 4f + SideDir() * spanOfHandle * 2f;
                        break;
                    case 1:
                        pos = Center() + UpDir() * spanOfHandle * 5f + SideDir() * spanOfHandle * 3f;
                        break;
                    default:
                        pos = Center();
                        break;
                }
            }
            else if (nbRightHandleSlingshot.Contains(index))
            {
                switch (nbRightHandleSlingshot.IndexOf(index))
                {
                    case 0:
                        pos = Center() + UpDir() * spanOfHandle * 4f + SideDir() * spanOfHandle * (-2f);
                        break;
                    case 1:
                        pos = Center() + UpDir() * spanOfHandle * 5f + SideDir() * spanOfHandle * (-3f);
                        break;
                    default:
                        pos = Center();
                        break;
                }
            }
            else if (nbStringSlingshot.Contains(index))
            {
                switch (nbStringSlingshot.IndexOf(index))
                {
                    case 0:
                        pos = Center() + UpDir() * spanOfHandle * 5f + SideDir() * spanOfString * 5f;
                        break;
                    case 1:
                        pos = Center() + UpDir() * spanOfHandle * 5f + SideDir() * spanOfString * (-5f);
                        break;
                    case 2:
                        pos = Center() + UpDir() * spanOfHandle * 5f + SideDir() * spanOfString * 3.5f;
                        break;
                    case 3:
                        pos = Center() + UpDir() * spanOfHandle * 5f + SideDir() * spanOfString * (-3.5f);
                        break;
                    case 4:
                        pos = Center() + UpDir() * spanOfHandle * 5f + SideDir() * spanOfString * (-2f);
                        break;
                    case 5:
                        pos = Center() + UpDir() * spanOfHandle * 5f + SideDir() * spanOfString * 2f;
                        break;
                    default:
                        pos = Center();
                        break;
                }
            }
            else
            {
                pos = Center() + UpDir() * spanOfHandle * 5f;
            }
            return pos;
        }

        public override Quaternion GetRot(int index, Rigidbody rb, BladePart part)
        {
            return Quaternion.LookRotation(ForwardDir(), rb.transform.position - Center());
        }

        public override void OnTriggerPressed()
        {
            base.OnTriggerPressed();
            if (!IsButtonPressed())
            {
                targetMode = true;
            }
        }

        public override void OnTriggerReleased()
        {
            base.OnTriggerReleased();
            if (!shardThrowed)
            {
                targetMode = false;
                stepTarget = 0;
            }
        }

        public override void OnTriggerHeld()
        {
            base.OnTriggerHeld();
        }

        public override void OnButtonPressed()
        {
            base.OnButtonPressed();
            if (IsTriggerPressed())
            {
                Reform();
            }
            else
            {
                bounceMode = true;
            }
        }

        public override void OnButtonReleased()
        {
            base.OnButtonReleased();
            if (!shardThrowed)
            {
                bounceMode = false;
                stepBounce = 0;
            }
        }

        private void Reform()
        {
            shardThrowed = false;
            grabbedShard = false;
            stopThrow = false;
            justHit = false;
            targetMode = false;
            bounceMode = false;
            disableColliders = true;
            stepBounce = 0;
            stepTarget = 0;
            nbBounce = 0;
            if (throwJoint != null)
            {
                UnityEngine.Object.Destroy(throwJoint);
                throwJoint = null;
            }
            sword.GetPart(nbProjectileSlingshot).item.rb.velocity = Vector3.zero;
            sword.GetPart(nbProjectileSlingshot).Depenetrate();
            sword.GetPart(nbProjectileSlingshot).Reform();
            sword.GetPart(nbProjectileSlingshot).item.IgnoreCollision();
        }

        public override void Update()
        {
            base.Update();
            emptyHandle.transform.position = Center() + UpDir() * spanOfHandle * 5f;
            emptyHandle.transform.rotation = Quaternion.LookRotation(ForwardDir());
            if (Math.Abs(Vector3.Distance(emptyHandle.transform.position, sword.GetPart(nbProjectileSlingshot).item.transform.position)) < 0.05f && disableColliders)
            {
                sword.GetPart(nbProjectileSlingshot).item.IgnoreCollision(false);
                disableColliders = false;
            }
            if (sword.GetPart(nbProjectileSlingshot).item.mainHandler == OtherHand() && !grabbedShard)
            {
                grabbedShard = true;
                sword.GetPart(nbProjectileSlingshot).Detach();
                throwJoint = Snippet.CreateSlingshotJoint(sword.GetPart(nbProjectileSlingshot).item.rb, emptyHandle, 130f, 0f);
            }
            if (grabbedShard && !shardThrowed && Math.Abs(Vector3.Distance(emptyHandle.transform.position, sword.GetPart(nbProjectileSlingshot).item.transform.position)) < 0.05f && sword.GetPart(nbProjectileSlingshot).item.mainHandler == null)
            {
                UnityEngine.Object.Destroy(throwJoint);
                shardThrowed = true;
                grabbedShard = false;
                sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(ForwardDir() * sword.GetPart(nbProjectileSlingshot).item.rb.velocity.magnitude * 5f, ForceMode.Impulse);
                sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
            }
            if (shardThrowed && !stopThrow)
            {
                if (bounceMode)
                {
                    switch (stepBounce)
                    {
                        // search for part
                        case 0:
                            if (Snippet.RandomCreatureInRadius(sword.GetPart(nbProjectileSlingshot).item.rb.position, creatureTarget == null ? 20f : 10f, false, false, creatureTarget, true) != null)
                            {
                                nbBounce = 0;
                                creatureTarget = Snippet.RandomCreatureInRadius(sword.GetPart(nbProjectileSlingshot).item.rb.position, creatureTarget == null ? 20f : 10f, false, false, creatureTarget, true);
                                shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb] = Snippet.ClosestRagdollPart(sword.GetPart(nbProjectileSlingshot).item.rb.position, creatureTarget, 0b11111111111, partTarget);
                                if (justHit && partTarget != null)
                                {
                                    sword.GetPart(nbProjectileSlingshot).item.rb.AddForce((partTarget.transform.position - sword.GetPart(nbProjectileSlingshot).item.transform.position).normalized * 0.1f, ForceMode.Impulse);
                                    justHit = false;
                                }
                                partTarget = shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb];
                                initialDistance = Vector3.Distance(sword.GetPart(nbProjectileSlingshot).item.rb.position, shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb].transform.position);
                                stepBounce = 1;
                            }
                            else
                            {
                                sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(sword.GetPart(nbProjectileSlingshot).item.rb.velocity.normalized * sword.GetPart(nbProjectileSlingshot).item.rb.velocity.magnitude * 1f, ForceMode.Impulse);
                                sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
                                if (nbBounce >= nbMaxBounce && justHit)
                                {
                                    Reform();
                                }
                            }
                            break;
                        // Go to part
                        case 1:
                            sword.GetPart(nbProjectileSlingshot).item.rb.velocity = Snippet.HomingTarget(sword.GetPart(nbProjectileSlingshot).item.rb, shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb].transform.position, initialDistance, 20f, 1f);
                            sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
                            break;
                    }
                }
                else if (targetMode)
                {
                    GameManager.local.StartCoroutine(TargetMode());
                }
                else
                {
                    stopThrow = true;
                }
            }
        }

        public IEnumerator TargetMode()
        {
            switch (stepTarget)
            {
                // search for part
                case 0:
                    if (Snippet.ClosestCreatureInRadius(sword.GetPart(nbProjectileSlingshot).item.rb.position, creatureTarget == null ? 20f : 10f, false, false) != null)
                    {
                        creatureTarget = Snippet.ClosestCreatureInRadius(sword.GetPart(nbProjectileSlingshot).item.rb.position, creatureTarget ? 20f : 10f, false, false);
                        shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb] = Snippet.ClosestRagdollPart(sword.GetPart(nbProjectileSlingshot).item.rb.position, creatureTarget, 0b00000000010);
                        partTarget = shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb];
                        initialDistance = Vector3.Distance(sword.GetPart(nbProjectileSlingshot).item.rb.position, shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb].transform.position);
                        stepTarget = 1;
                    }
                    else
                    {
                        sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(sword.GetPart(nbProjectileSlingshot).item.rb.velocity.normalized * sword.GetPart(nbProjectileSlingshot).item.rb.velocity.magnitude * 1f, ForceMode.Impulse);
                        sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
                        if (justHit)
                        {
                            Reform();
                        }
                    }
                    yield break;
                // Go to part
                case 1:
                    sword.GetPart(nbProjectileSlingshot).item.rb.velocity = Snippet.HomingTarget(sword.GetPart(nbProjectileSlingshot).item.rb, shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb].transform.position, initialDistance, 20f, 1f);
                    sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
                    yield break;
                // Pause
                case 2:
                    yield return new WaitForSeconds(0.2f);
                    stepTarget = 0;
                    yield break;
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
            if (throwJoint != null)
            {
                UnityEngine.GameObject.Destroy(throwJoint);
                throwJoint = null;
            }
            if (emptyHandle != null)
            {
                UnityEngine.GameObject.Destroy(emptyHandle);
                emptyHandle = null;
            }
            targetMode = false;
            bounceMode = false;
            grabbedShard = false;
            shardThrowed = false;
            stopThrow = false;
            justHit = true;
            disableColliders = false;
            stepTarget = 0;
            stepBounce = 0;
            sword.GetPart(nbProjectileSlingshot).item.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionStartEvent;
        }

        public override bool GetUseAnnotationShown() => true;
        public override bool GetAltUseAnnotationShown() => true;
        public override string GetUseAnnotation()
        {
            if (grabbedShard && !shardThrowed)
            {
                if (IsTriggerPressed() && !IsButtonPressed())
                {
                    return "Release it while maintaining the trigger launch a targeting projectile";
                }
                else if (!IsTriggerPressed() && IsButtonPressed())
                {
                    return "Release it while maintaining the trigger launch a bouncing projectile";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                if (IsTriggerPressed() && !IsButtonPressed())
                {
                    return "Grab the shard and pull it to launch a targeting projectile";
                }
                else if (!IsTriggerPressed() && IsButtonPressed())
                {
                    return "Grab the shard and pull it to launch a bouncing projectile";
                }
                else
                {
                    return "";
                }
            }
        }
        public override string GetAltUseAnnotation()
        {
            if (shardThrowed)
            {
                if (!IsTriggerPressed() && !IsButtonPressed())
                {
                    return "Hold trigger then tap the spell wheel button to recall the shard";
                }
                else if (IsTriggerPressed() && !IsButtonPressed())
                {
                    return "Tap the spell wheel button to recall the shard";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        public override bool ShouldReform(BladePart part) => part != sword.GetPart(nbProjectileSlingshot);

    }
}
