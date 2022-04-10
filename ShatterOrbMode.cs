﻿using System;
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
        private float rotation = 0f;
        private float rotation2 = 0f;
        private Vector3 originOfOrb;
        private List<Creature> targetCreatures;
        private Dictionary<RagdollPart, Rigidbody> partToShard = new Dictionary<RagdollPart, Rigidbody>();
        private Dictionary<Rigidbody, RagdollPart> shardToPart = new Dictionary<Rigidbody, RagdollPart>();
        private Dictionary<Rigidbody, float> initialDistanceOfTarget = new Dictionary<Rigidbody, float>();
        private List<RagdollPart> targetParts;
        private List<Rigidbody> orbPart;
        private bool isTargeting = false;
        private bool isThrowing = false;
        private bool shieldMode = false;
        private int partPicked = 7;
        private float shieldSize = 0.75f;
        private float orbRadius = 0.2f;
        public override int TargetPartNum() => partPicked;

        public override void Enter(Shatterblade.Shatterblade sword)
        {
            orbPart = sword.jointRBs.Where(rb => rb.name != $"Blade_{partPicked}").ToList();
            base.Enter(sword);
            targetCreatures = new List<Creature>();
            targetParts = new List<RagdollPart>();
            isTargeting = false;
            isThrowing = false;
        }

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            originOfOrb = Center() + ForwardDir() * 0.5f;
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
                        return originOfOrb + Quaternion.AngleAxis((float)index / orbPart.Count() * 180f, UpDir()) * Quaternion.AngleAxis((float)index / orbPart.Count() * 360f + rotation, ForwardDir()) * UpDir() * orbRadius;
                    }
                    else
                    {
                        return originOfOrb + Quaternion.AngleAxis((float)index / orbPart.Count() * 180f, -UpDir()) * Quaternion.AngleAxis((float)index / orbPart.Count() * 360f + rotation, ForwardDir()) * -UpDir() * orbRadius;
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
            rotation += Time.deltaTime * 480f;
            rotation2 += Time.deltaTime * 720f;
        }

        public override void OnTriggerPressed()
        {
            base.OnTriggerPressed();
            if (!isTargeting && !isThrowing)
            {
                targetCreatures = Snippet.CreaturesInRadiusMinusPlayer(originOfOrb, 5f).Where(cr => cr.state != Creature.State.Dead).ToList();
                if (targetCreatures != null)
                {
                    isTargeting = true;
                }
            }

            if (targetCreatures.Count() != 0 && isTargeting)
            {
                int numberOfShardPerCreature = Math.DivRem(orbPart.Count(), targetCreatures.Count(), out int remains);
                int nbShard = 0;
                RagdollPart part;
                foreach (Creature creature in targetCreatures)
                {
                    for (int i = 0; i < numberOfShardPerCreature; i++)
                    {
                        part = creature.GetRandomRagdollPart();
                        partToShard[part] = orbPart[nbShard];
                        shardToPart[partToShard[part]] = part;
                        initialDistanceOfTarget[orbPart[nbShard]] = Vector3.Distance(part.transform.position, sword.rbMap[orbPart[nbShard]].item.rb.position);
                        sword.rbMap[orbPart[nbShard]].Detach();
                        nbShard++;
                    }
                    if (remains != 0 && (targetCreatures.Count() - 1) == targetCreatures.LastIndexOf(creature))
                    {
                        for (int i = 0; i < remains; i++)
                        {
                            part = creature.GetRandomRagdollPart();
                            partToShard[part] = orbPart[nbShard];
                            shardToPart[partToShard[part]] = part;
                            initialDistanceOfTarget[orbPart[nbShard]] = Vector3.Distance(part.transform.position, sword.rbMap[orbPart[nbShard]].item.rb.position);
                            sword.rbMap[orbPart[nbShard]].Detach();
                            nbShard++;
                        }
                    }
                    if (nbShard >= orbPart.Count())
                    {
                        isThrowing = true;
                        isTargeting = false;
                        break;
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
                    sword.rbMap[rb].item.rb.velocity = Snippet.HomingTarget(sword.rbMap[rb].item.rb, shardToPart[rb].transform.position, initialDistanceOfTarget[rb], 50f, orbRadius);
                    sword.rbMap[rb].item.Throw(1, Item.FlyDetection.Forced);
                }
            }
        }

        public override void OnTriggerReleased()
        {
            base.OnTriggerReleased();
            foreach (Rigidbody rb in orbPart)
            {
                sword.rbMap[rb].Reform();
            }
            targetCreatures.Clear();
            targetParts.Clear();
            initialDistanceOfTarget.Clear();
            isTargeting = false;
            isThrowing = false;
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

        public override void OnButtonPressed()
        {
            base.OnButtonPressed();
            shieldMode ^= true;
        }

        public override bool ShouldReform(BladePart part) => part == sword.GetPart(partPicked);


    }
}
