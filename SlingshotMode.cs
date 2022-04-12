using Shatterblade;
using Shatterblade.Modes;
using System;
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
        private ConfigurableJoint throwJoint;
        private Rigidbody emptyHandle;
        private float spanOfString = 0.05f;
        private float spanOfHangle = 0.1f;
        private bool reformShard = false;
        private bool targetMode = false;
        private bool bounceMode = false;
        private bool grabbedShard = false;
        private bool shardThrowed = false;
        private bool targetAcquired = false;
        private bool stopThrow = false;
        private bool firstThrow = true;
        private float initialDistance;

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

        public void OnRagdollHit(RagdollPart ragdollPart, CollisionInstance hit)
        {
            if (ragdollPart.ragdoll.creature != Player.local.creature)
            {
                if (targetMode && (ragdollPart.type == RagdollPart.Type.Neck || ragdollPart.type == RagdollPart.Type.Torso || ragdollPart.type == RagdollPart.Type.Head))
                {
                    ragdollPart.ragdoll.headPart.Slice();
                    ragdollPart.ragdoll.creature.Kill();
                    sword.GetPart(nbProjectileSlingshot).item.Depenetrate();
                    targetAcquired = false;
                    firstThrow = false;
                }
                if (bounceMode)
                {
                    sword.GetPart(nbProjectileSlingshot).item.Depenetrate();
                    sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(Vector3.Reflect(hit.impactVelocity.normalized, hit.contactNormal) * 1.2f, ForceMode.VelocityChange);
                    targetAcquired = false;
                    firstThrow = false;
                }
            }
        }

        public void OnItemHit(Item item, CollisionInstance hit)
        {
            if (bounceMode)
            {
                sword.GetPart(nbProjectileSlingshot).item.Depenetrate();
                sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(Vector3.Reflect(hit.impactVelocity.normalized, hit.contactNormal) * 1.2f, ForceMode.VelocityChange);
                targetAcquired = false;
                firstThrow = false;
            }
        }
        public void OnOtherHit(CollisionInstance hit)
        {
            if (bounceMode)
            {
                sword.GetPart(nbProjectileSlingshot).item.Depenetrate();
                sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(Vector3.Reflect(hit.impactVelocity.normalized, hit.contactNormal) * 1.2f, ForceMode.VelocityChange);
                targetAcquired = false;
                firstThrow = false;
            }
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
                        pos = Center() + UpDir() * spanOfHangle * 1f;
                        break;
                    case 1:
                        pos = Center() + UpDir() * spanOfHangle * 2f;
                        break;
                    case 2:
                        pos = Center() + UpDir() * spanOfHangle * 3f;
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
                        pos = Center() + UpDir() * spanOfHangle * 4f + SideDir() * spanOfHangle * 2f;
                        break;
                    case 1:
                        pos = Center() + UpDir() * spanOfHangle * 5f + SideDir() * spanOfHangle * 3f;
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
                        pos = Center() + UpDir() * spanOfHangle * 4f + SideDir() * spanOfHangle * (-2f);
                        break;
                    case 1:
                        pos = Center() + UpDir() * spanOfHangle * 5f + SideDir() * spanOfHangle * (-3f);
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
                        pos = Center() + UpDir() * spanOfHangle * 5f + SideDir() * spanOfString * 5f;
                        break;
                    case 1:
                        pos = Center() + UpDir() * spanOfHangle * 5f + SideDir() * spanOfString * (-5f);
                        break;
                    case 2:
                        pos = Center() + UpDir() * spanOfHangle * 5f + SideDir() * spanOfString * 3.5f;
                        break;
                    case 3:
                        pos = Center() + UpDir() * spanOfHangle * 5f + SideDir() * spanOfString * (-3.5f);
                        break;
                    case 4:
                        pos = Center() + UpDir() * spanOfHangle * 5f + SideDir() * spanOfString * (-2f);
                        break;
                    case 5:
                        pos = Center() + UpDir() * spanOfHangle * 5f + SideDir() * spanOfString * 2f;
                        break;
                    default:
                        pos = Center();
                        break;
                }
            }
            else
            {
                pos = Center() + UpDir() * spanOfHangle * 5f;
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
            targetMode = false;
            targetAcquired = false;
        }

        public override void OnTriggerHeld()
        {
            base.OnTriggerHeld();
            if (IsButtonPressed() && reformShard)
            {
                if (throwJoint != null)
                {
                    UnityEngine.Object.Destroy(throwJoint);
                    throwJoint = null;
                }
                sword.GetPart(nbProjectileSlingshot).Depenetrate();
                sword.GetPart(nbProjectileSlingshot).Reform();
                reformShard = false;
                Debug.Log("Reformed ! ");
            }
        }

        public override void OnButtonPressed()
        {
            base.OnButtonPressed();
            if (IsTriggerPressed())
            {
                reformShard = true;
                shardThrowed = false;
                grabbedShard = false;
                stopThrow = false;
                targetAcquired = false;
                firstThrow = true;
                targetMode = false;
                bounceMode = false;
                Debug.Log("Ask for Reform ! ");
            }
            else
            {
                bounceMode = true;
                Debug.Log("Bounce ON ! ");
            }
        }

        public override void OnButtonReleased()
        {
            base.OnButtonReleased();
            bounceMode = false;
            Debug.Log("Bounce OFF ! ");
        }


        public override void Update()
        {
            base.Update();
            emptyHandle.transform.position = Center() + UpDir() * spanOfHangle * 5f;
            //emptyHandle.transform.rotation = Quaternion.LookRotation(ForwardDir(), SideDir());
            emptyHandle.transform.rotation = Quaternion.LookRotation(ForwardDir());
            //emptyHandle.transform.rotation = Quaternion.LookRotation(ForwardDir(), sword.GetPart(nbProjectileSlingshot).item.rb.transform.position - Center()) * Quaternion.Inverse(sword.GetPart(nbProjectileSlingshot).item.GetFlyDirRefLocalRotation());
            if (sword.GetPart(nbProjectileSlingshot).item.mainHandler == OtherHand() && !grabbedShard)
            {
                grabbedShard = true;
                sword.GetPart(nbProjectileSlingshot).Detach();
                throwJoint = Snippet.CreateSlingshotJoint(sword.GetPart(nbProjectileSlingshot).item.rb, emptyHandle, 130f, 0f);
                Debug.Log("Detached and jointed");
            }

            if (grabbedShard && !shardThrowed && Math.Abs(Vector3.Distance(emptyHandle.transform.position, sword.GetPart(nbProjectileSlingshot).item.transform.position)) < 0.03f && sword.GetPart(nbProjectileSlingshot).item.mainHandler == null)
            {
                UnityEngine.Object.Destroy(throwJoint);
                shardThrowed = true;
                grabbedShard = false;
                Debug.Log("Unjointed and throwed");
            }
            if (shardThrowed && !stopThrow)
            {
                if (bounceMode)
                {
                    if (!targetAcquired)
                    {
                        if (firstThrow)
                        {
                            sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(ForwardDir() * sword.GetPart(nbProjectileSlingshot).item.rb.velocity.magnitude * 4f, ForceMode.Impulse);
                            sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
                            Debug.Log("First throw");
                            if (Snippet.ClosestRagdollPart(sword.GetPart(nbProjectileSlingshot).item.rb.position, 10f, 0b11111111111) != null)
                            {
                                shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb] = Snippet.ClosestRagdollPart(sword.GetPart(nbProjectileSlingshot).item.rb.position, 10f, 0b00000000010);
                                initialDistance = Vector3.Distance(sword.GetPart(nbProjectileSlingshot).item.rb.position, shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb].transform.position);
                                targetAcquired = true;
                                Debug.Log("First throw & target acquired");
                            }
                        }
                        else
                        {
                            if (partTarget != null)
                            {
                                if (shardToPart.TryGetValue(sword.GetPart(nbProjectileSlingshot).item.rb, out partTarget))
                                {
                                    // NULL REFFERENCE when creature is off !
                                    sword.GetPart(nbProjectileSlingshot).item.rb.AddForce((partTarget.transform.position - sword.GetPart(nbProjectileSlingshot).item.transform.position).normalized * 5f, ForceMode.Impulse);
                                    Debug.Log("Homing not first throw");
                                }
                            }
                        }
                        if (shardToPart.TryGetValue(sword.GetPart(nbProjectileSlingshot).item.rb, out partTarget))
                        {
                            if (Snippet.RandomCreatureInRadius(sword.GetPart(nbProjectileSlingshot).item.rb.position, 10f, false, false, partTarget.ragdoll.creature) != null)
                            {
                                Creature target = Snippet.RandomCreatureInRadius(sword.GetPart(nbProjectileSlingshot).item.rb.position, 10f, false, false, partTarget.ragdoll.creature);
                                if (target != null)
                                {
                                    Debug.Log("Homing not first throw GET RANDOM PART");
                                    shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb] = Snippet.GetRandomRagdollPart(target);
                                    initialDistance = Vector3.Distance(sword.GetPart(nbProjectileSlingshot).item.rb.position, shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb].transform.position);
                                    targetAcquired = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No Target !");
                        sword.GetPart(nbProjectileSlingshot).item.rb.velocity = Snippet.HomingTarget(sword.GetPart(nbProjectileSlingshot).item.rb, shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb].transform.position, initialDistance, 20f);
                        sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
                    }
                }
                else if (targetMode)
                {
                    if (!targetAcquired)
                    {
                        if (firstThrow)
                        {
                            sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(ForwardDir() * sword.GetPart(nbProjectileSlingshot).item.rb.velocity.magnitude * 4f, ForceMode.Impulse);
                            sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
                        }
                        else
                        {
                            if (shardToPart.TryGetValue(sword.GetPart(nbProjectileSlingshot).item.rb, out partTarget))
                            {
                                sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(Vector3.up * 5f + (partTarget.transform.position - sword.GetPart(nbProjectileSlingshot).item.transform.position).normalized * 5f, ForceMode.Impulse);
                            }
                        }
                        if (Snippet.ClosestRagdollPart(sword.GetPart(nbProjectileSlingshot).item.rb.position, 10f, 0b00000000010) != null)
                        {
                            shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb] = Snippet.ClosestRagdollPart(sword.GetPart(nbProjectileSlingshot).item.rb.position, 10f, 0b00000000010);
                            initialDistance = Vector3.Distance(sword.GetPart(nbProjectileSlingshot).item.rb.position, shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb].transform.position);
                            targetAcquired = true;
                        }
                    }
                    else
                    {
                        sword.GetPart(nbProjectileSlingshot).item.rb.velocity = Snippet.HomingTarget(sword.GetPart(nbProjectileSlingshot).item.rb, shardToPart[sword.GetPart(nbProjectileSlingshot).item.rb].transform.position, initialDistance, 20f);
                        sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
                    }
                }
                else
                {
                    sword.GetPart(nbProjectileSlingshot).item.rb.AddForce(ForwardDir() * sword.GetPart(nbProjectileSlingshot).item.rb.velocity.magnitude * 5f, ForceMode.Impulse);
                    sword.GetPart(nbProjectileSlingshot).item.Throw(1, Item.FlyDetection.Forced);
                    stopThrow = true;
                    Debug.Log("THROW !");
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
            reformShard = false;
            targetMode = false;
            bounceMode = false;
            grabbedShard = false;
            shardThrowed = false;
            targetAcquired = false;
            stopThrow = false;
            firstThrow = true;
            sword.GetPart(nbProjectileSlingshot).item.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionStartEvent;
        }

        public override bool ShouldReform(BladePart part) => part != sword.GetPart(nbProjectileSlingshot);

    }
}
