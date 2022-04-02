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
    public class ShatterOrbMode : GrabbedShardMode
    {
        private float rotation;
        private float rotation2;
        private Vector3 originOfOrb;
        private List<Creature> targetCreatures;
        private Dictionary<RagdollPart, Rigidbody> partToShard = new Dictionary<RagdollPart, Rigidbody>();
        private Dictionary<Rigidbody, RagdollPart> shardToPart = new Dictionary<Rigidbody, RagdollPart>();
        private Dictionary<Rigidbody, float> initialDistanceOfTarget = new Dictionary<Rigidbody, float>();
        private Dictionary<Rigidbody, float> distanceOfTarget = new Dictionary<Rigidbody, float>();
        private List<RagdollPart> targetParts;
        private List<Rigidbody> orbPart;
        private int numberOfShardPerCreature;
        private bool isTargeting = false;
        private bool isThrowing = false;
        private bool shieldMode = false;
        private int partPicked = 7;
        private float shieldSize = 0.75f;
        public override int TargetPartNum() => partPicked;

        public override void Enter(Shatterblade.Shatterblade sword)
        {
            orbPart = sword.jointRBs.Where(rb => rb.name != $"Blade_{partPicked}").ToList();
            Debug.Log("ShatterOrb : Nb part : " + orbPart.Count());
            base.Enter(sword);
            targetCreatures = new List<Creature>();
            targetParts = new List<RagdollPart>();
            isTargeting = false;
            isThrowing = false;
            foreach (Rigidbody part in orbPart)
            {
                sword.rbMap[part].Detach();
                sword.rbMap[part].Reform();
            }
        }

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            if (shieldMode)
            {
                if (index % 2 == 0)
                {
                    return Player.local.creature.GetChest() + Quaternion.AngleAxis((float)index / orbPart.Count() * 180f + rotation2, UpDir()) * Quaternion.AngleAxis((float)index / orbPart.Count() * 360f + rotation, ForwardDir()) * UpDir() * shieldSize;
                }
                else
                {
                    return Player.local.creature.GetChest() + Quaternion.AngleAxis((float)index / orbPart.Count() * 180f + rotation2, -UpDir()) * Quaternion.AngleAxis((float)index / orbPart.Count() * 360f + rotation, ForwardDir()) * -UpDir() * shieldSize;
                }
            }
            else
            {
                if (!isThrowing)
                {
                    if (index % 2 == 0)
                    {
                        return originOfOrb + Quaternion.AngleAxis((float)index / orbPart.Count() * 180f, UpDir()) * Quaternion.AngleAxis((float)index / orbPart.Count() * 360f + rotation, ForwardDir()) * UpDir() * 0.2f;
                    }
                    else
                    {
                        return originOfOrb + Quaternion.AngleAxis((float)index / orbPart.Count() * 180f, -UpDir()) * Quaternion.AngleAxis((float)index / orbPart.Count() * 360f + rotation, ForwardDir()) * -UpDir() * 0.2f;
                    }
                }
                else
                {
                    return originOfOrb;
                }
            }
        }

        public override Quaternion GetRot(int index, Rigidbody rb, BladePart part)
        {
            return Quaternion.LookRotation(ForwardDir(), rb.transform.position - Center());
        }

        public override void Update()
        {
            base.Update();
            originOfOrb = Center() + ForwardDir() * .5f;
            rotation += Time.deltaTime * 480;
            rotation2 += Time.deltaTime * 720;
            shieldSize = Snippet.PingPongValue(shieldSize, shieldSize, 5f);
        }

        public override void OnTriggerPressed()
        {
            base.OnTriggerPressed();
            if (!isTargeting && !isThrowing)
            {
                targetCreatures = Snippet.CreatureInRadiusMinusPlayer(originOfOrb, 10f).Where(cr => cr.state != Creature.State.Dead).ToList();
                if (targetCreatures != null)
                {
                    isTargeting = true;
                    Debug.Log("ShatterOrb : Targeting true");
                }
            }

            if (targetCreatures.Count() != 0)
            {
                numberOfShardPerCreature = orbPart.Count() / targetCreatures.Count();
                if (isTargeting && !isThrowing)
                {
                    int nbShard = 0;
                    RagdollPart part;
                    foreach (Creature creature in targetCreatures)
                    {
                        for (int i = 0; i < numberOfShardPerCreature; i++)
                        {
                            part = creature.GetRandomRagdollPart();
                            partToShard[part] = orbPart[nbShard];
                            shardToPart[partToShard[part]] = part;
                            initialDistanceOfTarget[orbPart[nbShard]] = Vector3.Distance(part.transform.position, originOfOrb);
                            distanceOfTarget[orbPart[nbShard]] = initialDistanceOfTarget[orbPart[nbShard]];
                            sword.rbMap[orbPart[nbShard]].Detach();
                            nbShard++;
                        }
                        if (nbShard > orbPart.Count())
                        {
                            isThrowing = true;
                            isTargeting = false;
                            Debug.Log("ShatterOrb : Throwing true");
                            break;
                        }
                    }
                    foreach (Rigidbody rigidbody in orbPart)
                    {
                        Debug.Log("ShatterOrb Part : " + shardToPart[rigidbody].name + " ; Shard : " + sword.rbMap[rigidbody].name);
                    }
                }
            }
        }

        public override void OnTriggerHeld()
        {
            base.OnTriggerHeld();
            if (isThrowing)
            {
                foreach (Rigidbody rb in orbPart)
                {

                    distanceOfTarget[rb] = Vector3.Distance(sword.rbMap[rb].item.rb.position, shardToPart[rb].transform.position);
                    sword.rbMap[rb].item.rb.velocity = Snippet.HomingTarget(sword.rbMap[rb].item.rb, shardToPart[rb].transform.position, initialDistanceOfTarget[rb], 60f);
                    sword.rbMap[rb].item.Throw(1, Item.FlyDetection.Forced);
                }
                isThrowing = false;
            }
        }

        public override void OnTriggerReleased()
        {
            base.OnTriggerReleased();
            isTargeting = false;
            isThrowing = false;
            Debug.Log("ShatterOrb : Throwing false");
            targetCreatures.Clear();
            targetParts.Clear();
            initialDistanceOfTarget.Clear();
            foreach (Rigidbody part in orbPart)
            {
                sword.rbMap[part].Reform();
            }
        }

        public override void JointModifier(ConfigurableJoint joint, BladePart part)
        {
            JointDrive posDrive = new JointDrive
            {
                positionSpring = 2000,
                positionDamper = 10000,
                maximumForce = sword.module.jointMaxForce
            };
            JointDrive rotDrive = new JointDrive
            {
                positionSpring = 2000,
                positionDamper = 10000,
                maximumForce = sword.module.jointMaxForce
            };
            joint.xDrive = posDrive;
            joint.yDrive = posDrive;
            joint.zDrive = posDrive;
            joint.angularXDrive = rotDrive;
            joint.angularYZDrive = rotDrive;
        }

        public override void OnButtonPressed()
        {
            base.OnButtonPressed();
            shieldMode ^= true;
        }

        public override bool ShouldReform(BladePart part) => part == sword.GetPart(partPicked);

    }
}
