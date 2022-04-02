using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using SnippetCode;
using System.Collections;
using Object = System.Object;
using Random = UnityEngine.Random;


namespace SnippetCode
{
    public static class SnippetCode
    {
        
    }
}

public static class Snippet
{


    public static float NegPow(this float input, float power) => Mathf.Pow(input, power) * (input / Mathf.Abs(input));
    public static float Pow(this float input, float power) => Mathf.Pow(input, power);
    public static float Sqrt(this float input) => Mathf.Sqrt(input);
    public static float Clamp(this float input, float low, float high) => Mathf.Clamp(input, low, high);
    public static float Remap(this float input, float inLow, float inHigh, float outLow, float outHigh)
        => (input - inLow) / (inHigh - inLow) * (outHigh - outLow) + outLow;

    public static float RemapClamp(this float input, float inLow, float inHigh, float outLow, float outHigh)
        => (Mathf.Clamp(input, inLow, inHigh) - inLow) / (inHigh - inLow) * (outHigh - outLow) + outLow;

    public static float Remap01(this float input, float inLow, float inHigh) => (input - inLow) / (inHigh - inLow);

    public static float RemapClamp01(this float input, float inLow, float inHigh)
        => (Mathf.Clamp(input, inLow, inHigh) - inLow) / (inHigh - inLow);

    public static float OneMinus(this float input) => Mathf.Clamp01(1 - input);

    public static float Randomize(this float input, float range) => input * Random.Range(1f - range, 1f + range);

    public static float Curve(this float time, params float[] values)
    {
        var curve = new AnimationCurve();
        int i = 0;
        foreach (var value in values)
        {
            curve.AddKey(i / ((float)values.Length - 1), value);
            i++;
        }

        return curve.Evaluate(time);
    }

    public static float MapOverCurve(this float time, params Tuple<float, float>[] points)
    {
        var curve = new AnimationCurve();
        foreach (var point in points)
        {
            curve.AddKey(new Keyframe(point.Item1, point.Item2));
        }

        return curve.Evaluate(time);
    }

    public static float MapOverCurve(this float time, params Tuple<float, float, float, float>[] points)
    {
        var curve = new AnimationCurve();
        foreach (var point in points)
        {
            curve.AddKey(new Keyframe(point.Item1, point.Item2, point.Item3, point.Item4));
        }

        return curve.Evaluate(time);
    }



    // Original idea from walterellisfun on github: https://github.com/walterellisfun/ConeCast/blob/master/ConeCastExtension.cs
    /// <summary>
    /// Like SphereCastAll but in a cone
    /// </summary>
    /// <param name="origin">Origin position</param>
    /// <param name="maxRadius">Maximum cone radius</param>
    /// <param name="direction">Cone direction</param>
    /// <param name="maxDistance">Maximum cone distance</param>
    /// <param name="coneAngle">Cone angle</param>
    public static RaycastHit[] ConeCastAll(this Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle, int layer = Physics.DefaultRaycastLayers)
    {
        RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin, maxRadius, direction, maxDistance, layer, QueryTriggerInteraction.Ignore);
        List<RaycastHit> coneCastHitList = new List<RaycastHit>();

        if (sphereCastHits.Length > 0)
        {
            for (int i = 0; i < sphereCastHits.Length; i++)
            {
                Vector3 hitPoint = sphereCastHits[i].point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);
                float multiplier = 1f;
                if (directionToHit.magnitude < 2f)
                    multiplier = 4f;
                bool hitRigidbody = sphereCastHits[i].rigidbody is Rigidbody rb
                                    && Vector3.Angle(direction, rb.transform.position - origin) < coneAngle * multiplier;

                if (angleToHit < coneAngle * multiplier || hitRigidbody)
                {
                    coneCastHitList.Add(sphereCastHits[i]);
                }
            }
        }
        return coneCastHitList.ToArray();
    }

    /// <summary>
    /// Get a component from the gameobject, or create it if it doesn't exist
    /// </summary>
    /// <typeparam name="T">The component type</typeparam>
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        return obj.GetComponent<T>() ?? obj.AddComponent<T>();
    }

    /// <summary>
    /// Vector pointing away from the palm
    /// </summary>
    public static Vector3 PalmDir(this RagdollHand hand) => -hand.transform.forward;

    /// <summary>
    /// Vector pointing in the direction of the thumb
    /// </summary>
    public static Vector3 ThumbDir(this RagdollHand hand) => (hand.side == Side.Right) ? hand.transform.up : -hand.transform.up;

    /// <summary>
    /// Vector pointing away in the direction of the fingers
    /// </summary>
    public static Vector3 PointDir(this RagdollHand hand) => -hand.transform.right;

    /// <summary>
    /// Get a point above the player's hand
    /// </summary>
    public static Vector3 PosAboveBackOfHand(this RagdollHand hand, float factor = 1f) => hand.transform.position - hand.transform.right * 0.1f * factor + hand.transform.forward * 0.2f * factor;

    public static void SetVFXProperty<T>(this EffectInstance effect, string name, T data)
    {
        if (effect == null)
            return;
        if (data is Vector3 v)
        {
            foreach (EffectVfx effectVfx in effect.effects.Where<Effect>((Func<Effect, bool>)(fx => fx is EffectVfx effectVfx17 && effectVfx17.vfx.HasVector3(name))))
                effectVfx.vfx.SetVector3(name, v);
        }
        else if (data is float f2)
        {
            foreach (EffectVfx effectVfx2 in effect.effects.Where<Effect>((Func<Effect, bool>)(fx => fx is EffectVfx effectVfx18 && effectVfx18.vfx.HasFloat(name))))
                effectVfx2.vfx.SetFloat(name, f2);
        }
        else if (data is int i3)
        {
            foreach (EffectVfx effectVfx2 in effect.effects.Where<Effect>((Func<Effect, bool>)(fx => fx is EffectVfx effectVfx19 && effectVfx19.vfx.HasInt(name))))
                effectVfx2.vfx.SetInt(name, i3);
        }
        else if (data is bool b4)
        {
            foreach (EffectVfx effectVfx2 in effect.effects.Where<Effect>((Func<Effect, bool>)(fx => fx is EffectVfx effectVfx20 && effectVfx20.vfx.HasBool(name))))
                effectVfx2.vfx.SetBool(name, b4);
        }
        else
        {
            if (!(data is Texture t5))
                return;
            foreach (EffectVfx effectVfx2 in effect.effects.Where<Effect>((Func<Effect, bool>)(fx => fx is EffectVfx effectVfx21 && effectVfx21.vfx.HasTexture(name))))
                effectVfx2.vfx.SetTexture(name, t5);
        }
    }
    public static object GetVFXProperty(this EffectInstance effect, string name)
    {
        foreach (Effect effect1 in effect.effects)
        {
            if (effect1 is EffectVfx effectVfx1)
            {
                if (effectVfx1.vfx.HasFloat(name))
                    return effectVfx1.vfx.GetFloat(name);
                if (effectVfx1.vfx.HasVector3(name))
                    return effectVfx1.vfx.GetVector3(name);
                if (effectVfx1.vfx.HasBool(name))
                    return effectVfx1.vfx.GetBool(name);
                if (effectVfx1.vfx.HasInt(name))
                    return effectVfx1.vfx.GetInt(name);
            }
        }
        return null;
    }
    public static Vector3 zero = Vector3.zero;
    public static Vector3 one = Vector3.one;
    public static Vector3 forward = Vector3.forward;
    public static Vector3 right = Vector3.right;
    public static Vector3 up = Vector3.up;
    public static Vector3 back = Vector3.back;
    public static Vector3 left = Vector3.left;
    public static Vector3 down = Vector3.down;

    public static bool XBigger(this Vector3 vec) => Mathf.Abs(vec.x) > Mathf.Abs(vec.y) && Mathf.Abs(vec.x) > Mathf.Abs(vec.z);

    public static bool YBigger(this Vector3 vec) => Mathf.Abs(vec.y) > Mathf.Abs(vec.x) && Mathf.Abs(vec.y) > Mathf.Abs(vec.z);

    public static bool ZBigger(this Vector3 vec) => Mathf.Abs(vec.z) > Mathf.Abs(vec.x) && Mathf.Abs(vec.z) > Mathf.Abs(vec.y);

    public static Vector3 Velocity(this RagdollHand hand) => Player.local.transform.rotation * hand.playerHand.controlHand.GetHandVelocity();

    /// <summary>
    /// .Select(), but only when the output of the selection function is non-null
    /// </summary>
    public static IEnumerable<TOut> SelectNotNull<TIn, TOut>(this IEnumerable<TIn> enumerable, Func<TIn, TOut> func)
        => enumerable.Where(item => func(item) != null).Select(func);

    public static bool IsPlayer(this RagdollPart part) => part?.ragdoll?.creature.isPlayer == true;
    public static bool IsImportant(this RagdollPart part)
    {
        var type = part.type;
        return type == RagdollPart.Type.Head
               || type == RagdollPart.Type.Torso
               || type == RagdollPart.Type.LeftHand
               || type == RagdollPart.Type.RightHand
               || type == RagdollPart.Type.LeftFoot
               || type == RagdollPart.Type.RightFoot;
    }
    /// <summary>
    /// Get a creature's part from a PartType
    /// </summary>
    public static RagdollPart GetPart(this Creature creature, RagdollPart.Type partType)
        => creature.ragdoll.GetPart(partType);

    /// <summary>
    /// Get a creature's head
    /// </summary>
    public static RagdollPart GetHead(this Creature creature) => creature.ragdoll.headPart;

    /// <summary>
    /// Get a creature's torso
    /// </summary>
    public static RagdollPart GetTorso(this Creature creature) => creature.GetPart(RagdollPart.Type.Torso);

    public static Vector3 GetChest(this Creature creature) => Vector3.Lerp(creature.GetTorso().transform.position,
        creature.GetHead().transform.position, 0.5f);
    public static IEnumerable<Creature> CreaturesInRadius(this Vector3 position, float radius)
    {
        return Creature.allActive.Where(creature => (creature.GetChest() - position).sqrMagnitude < radius * radius);
    }

    public static IEnumerable<Creature> CreatureInRadiusMinusPlayer(this Vector3 position, float radius)
    {
        return Creature.allActive.Where(creature =>
            ((creature.GetChest() - position).sqrMagnitude < radius * radius) && !creature.isPlayer
        );
    }
    public static void Depenetrate(this Item item)
    {
        foreach (var handler in item.collisionHandlers)
        {
            foreach (var damager in handler.damagers)
            {
                damager.UnPenetrateAll();
            }
        }
    }
    /// <summary>
    /// Get a creature's random part
    /// </summary>
    /*
    Head
    Neck
    Torso
    LeftArm
    RightArm
    LeftHand
    RightHand
    LeftLeg
    RightLeg
    LeftFoot
    RightFoot
    */
    public static RagdollPart GetRandomRagdollPart(this Creature creature)
    {
        Array values = Enum.GetValues(typeof(RagdollPart.Type));
        return creature.ragdoll.GetPart((RagdollPart.Type)values.GetValue(UnityEngine.Random.Range(0, values.Length)));
    }

    public static bool returnWaveStarted()
    {
        int nbWaveStarted = 0;
        foreach (WaveSpawner waveSpawner in WaveSpawner.instances)
        {
            if (waveSpawner.isRunning)
            {
                nbWaveStarted++;
            }
        }
        return nbWaveStarted != 0 ? true : false;
    }

    public static Vector3 FromToDirection(this Vector3 from, Vector3 to)
    {
        return to - from;
    }
    /// <summary>
    /// Add a force that attracts when coef is positive and repulse when is negative
    /// </summary>
    public static void Attraction_Repulsion_Force(this Rigidbody rigidbody, Vector3 origin, Vector3 attractedRb, bool useDistance, float coef)
    {
        if (useDistance)
        {
            float distance = FromToDirection(attractedRb, origin).magnitude;
            Vector3 direction = FromToDirection(attractedRb, origin).normalized;
            rigidbody.AddForce(direction * (coef / distance) / (rigidbody.mass / 2), ForceMode.VelocityChange);
        }
        else
        {
            Vector3 direction = FromToDirection(attractedRb, origin).normalized;
            rigidbody.AddForce(direction * coef / (rigidbody.mass / 2), ForceMode.VelocityChange);
        }
    }
    /// <summary>
    /// Add a force that attracts when coef is positive and repulse when is negative
    /// </summary>
    public static void Attraction_Repulsion_ForceNoMass(this Rigidbody rigidbody, Vector3 origin, Vector3 attractedRb, bool useDistance, float coef)
    {
        if (useDistance)
        {
            float distance = FromToDirection(attractedRb, origin).magnitude;
            Vector3 direction = FromToDirection(attractedRb, origin).normalized;
            rigidbody.AddForce(direction * (coef / distance), ForceMode.VelocityChange);
        }
        else
        {
            Vector3 direction = FromToDirection(attractedRb, origin).normalized;
            rigidbody.AddForce(direction * coef, ForceMode.VelocityChange);
        }
    }

    public static Vector3 HomingThrow(Item item, Vector3 velocity, float homingAngle)
    {
        var hits = Physics.SphereCastAll(item.transform.position, 10, velocity, 10, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        var targets = hits.SelectNotNull(hit => hit.collider?.attachedRigidbody?.GetComponentInParent<Creature>())
            .Where(creature => creature != Player.currentCreature && creature.state != Creature.State.Dead)
            .Where(creature => Vector3.Angle(velocity, creature.ragdoll.headPart.transform.position - item.transform.position)
                 < homingAngle + 3 * Vector3.Distance(item.transform.position, Player.currentCreature.transform.position))
            .OrderBy(creature => Vector3.Angle(velocity, creature.ragdoll.headPart.transform.position - item.transform.position));
        var closeToAngle = targets.Where(creature => Vector3.Angle(velocity, creature.ragdoll.headPart.transform.position - item.transform.position) < 5);
        if (closeToAngle.Any())
        {
            targets = closeToAngle.OrderBy(creature => Vector3.Distance(item.transform.position, creature.ragdoll.headPart.transform.position));
        }
        var target = targets.FirstOrDefault();
        if (!target)
            return velocity;
        var extendedPoint = item.transform.position + velocity.normalized * Vector3.Distance(item.transform.position, target.ragdoll.GetPart(RagdollPart.Type.Torso).transform.position);
        var targetPart = target.ragdoll.parts.MinBy(part => Vector3.Distance(part.transform.position, extendedPoint));
        var vectorToTarget = targetPart.transform.position - item.transform.position;
        item.rb.velocity = Vector3.zero;
        velocity = vectorToTarget.normalized * velocity.magnitude;
        return velocity;
    }

    /// <summary>
    /// Return the minimum entry in an interator using a custom comparable function
    /// </summary>
    public static T MinBy<T>(this IEnumerable<T> enumerable, Func<T, IComparable> comparator)
    {
        if (!enumerable.Any())
            return default;
        return enumerable.Aggregate((curMin, x) => (curMin == null || (comparator(x).CompareTo(comparator(curMin)) < 0)) ? x : curMin);
    }

    public static Vector3[] CreateCircle(this Vector3 origin, Vector3 direction, float radius, int nbElementsAroundCircle)
    {
        Vector3[] positions = new Vector3[nbElementsAroundCircle];
        int angle = 360 / nbElementsAroundCircle;
        for (int i = 0; i < nbElementsAroundCircle; i++)
        {
            positions[i] = origin + direction * radius;
        }
        return positions;
    }
    public static void RotateCircle(this Vector3[] positions, Vector3 origin, Vector3 direction, float radius, int speed)
    {
        float rotationspeed = 0;
        rotationspeed += Time.deltaTime * speed;
    }

    public static ConfigurableJoint CreateJointToProjectileForCreatureAttraction(this Item projectile, RagdollPart attractedRagdollPart, ConfigurableJoint joint)
    {
        JointDrive jointDrive = new JointDrive();
        jointDrive.positionSpring = 1f;
        jointDrive.positionDamper = 0.2f;
        SoftJointLimit softJointLimit = new SoftJointLimit();
        softJointLimit.limit = 0.15f;
        SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        linearLimitSpring.spring = 1f;
        linearLimitSpring.damper = 0.2f;
        joint = attractedRagdollPart.gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.targetRotation = Quaternion.identity;
        joint.anchor = Vector3.zero;
        joint.connectedBody = projectile.GetComponent<Rigidbody>();
        joint.connectedAnchor = Vector3.zero;
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
        joint.linearLimitSpring = linearLimitSpring;
        joint.linearLimit = softJointLimit;
        joint.angularXLimitSpring = linearLimitSpring;
        joint.xDrive = jointDrive;
        joint.yDrive = jointDrive;
        joint.zDrive = jointDrive;
        joint.massScale = 10000f;
        joint.connectedMassScale = 0.00001f;
        return joint;
    }

    public static FixedJoint CreateStickyJointBetweenTwoRigidBodies(this Rigidbody startRB, Rigidbody targetRB, FixedJoint joint)
    {
        joint = targetRB.gameObject.AddComponent<FixedJoint>();
        joint.anchor = Vector3.zero;
        joint.connectedBody = startRB;
        joint.connectedAnchor = Vector3.zero;
        joint.massScale = 0.00001f;
        joint.connectedMassScale = 10000f;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;
        return joint;
    }

    public static void IgnoreCollider(this Ragdoll ragdoll, Collider collider, bool ignore = true)
    {
        foreach (var part in ragdoll.parts)
        {
            part.IgnoreCollider(collider, ignore);
        }
    }

    public static void IgnoreCollider(this RagdollPart part, Collider collider, bool ignore = true)
    {
        foreach (var itemCollider in part.colliderGroup.colliders)
        {
            Physics.IgnoreCollision(collider, itemCollider, ignore);
        }
    }

    public static void IgnoreCollider(this Item item, Collider collider, bool ignore)
    {
        foreach (var cg in item.colliderGroups)
        {
            foreach (var itemCollider in cg.colliders)
            {
                Physics.IgnoreCollision(collider, itemCollider, ignore);
            }
        }
    }

    public static void addIgnoreRagdollAndItemHoldingCollision(Item item, Creature creature)
    {
        foreach (ColliderGroup colliderGroup in item.colliderGroups)
        {
            foreach (Collider collider in colliderGroup.colliders)
                creature.ragdoll.IgnoreCollision(collider, true);
        }
        item.ignoredRagdoll = creature.ragdoll;

        if (creature.handLeft.grabbedHandle?.item != null)
        {
            foreach (ColliderGroup colliderGroup1 in item.colliderGroups)
            {
                foreach (Collider collider1 in colliderGroup1.colliders)
                {
                    foreach (ColliderGroup colliderGroup2 in creature.handLeft.grabbedHandle.item.colliderGroups)
                    {
                        foreach (Collider collider2 in colliderGroup2.colliders)
                            Physics.IgnoreCollision(collider1, collider2, true);
                    }
                }
            }
            item.ignoredItem = creature.handLeft.grabbedHandle.item;
        }

        if (creature.handRight.grabbedHandle?.item != null)
        {
            foreach (ColliderGroup colliderGroup1 in item.colliderGroups)
            {
                foreach (Collider collider1 in colliderGroup1.colliders)
                {
                    foreach (ColliderGroup colliderGroup2 in creature.handRight.grabbedHandle.item.colliderGroups)
                    {
                        foreach (Collider collider2 in colliderGroup2.colliders)
                            Physics.IgnoreCollision(collider1, collider2, true);
                    }
                }
            }
            item.ignoredItem = creature.handRight.grabbedHandle.item;
        }
    }

    /// <summary>
    /// return the head, torso, leftHand, rightHand, leftFoot and rightFoot of the creature
    /// </summary>
    public static List<RagdollPart> RagdollPartsImportantList(this Creature creature)
    {
        List<RagdollPart> ragdollPartsimportant = new List<RagdollPart> {
                creature.GetPart(RagdollPart.Type.Head),
                creature.GetPart(RagdollPart.Type.Torso),
                creature.GetPart(RagdollPart.Type.LeftHand),
                creature.GetPart(RagdollPart.Type.RightHand),
                creature.GetPart(RagdollPart.Type.LeftFoot),
                creature.GetPart(RagdollPart.Type.RightFoot)};
        return ragdollPartsimportant;
    }
    /// <summary>
    /// return the leftHand, rightHand, leftFoot and rightFoot of the creature
    /// </summary>
    public static List<RagdollPart> RagdollPartsExtremitiesBodyList(this Creature creature)
    {
        List<RagdollPart> ragdollPartsimportant = new List<RagdollPart> {
                creature.GetPart(RagdollPart.Type.LeftHand),
                creature.GetPart(RagdollPart.Type.RightHand),
                creature.GetPart(RagdollPart.Type.LeftFoot),
                creature.GetPart(RagdollPart.Type.RightFoot)};
        return ragdollPartsimportant;
    }

    public static Vector3 RandomPositionAroundCreatureInRadius(this Creature creature, float radius)
    {
        return creature.transform.position + new Vector3(UnityEngine.Random.Range(-radius, radius), 0, UnityEngine.Random.Range(-radius, radius));
    }

    public static Vector3 CalculatePositionFromAngleWithDistance(this Vector3 position, Vector3 forward, float angle, Vector3 axis, float distance)
    {
        return position + Quaternion.AngleAxis(angle, axis) * forward * distance;
    }

    public static void DebugPosition(this Vector3 position, string textToDisplay)
    {
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Position X : " + position.x.ToString() + "; Position Y : " + position.y.ToString() + "; Position Z : " + position.z.ToString());
    }
    public static void DebugRotation(this Quaternion rotation, string textToDisplay)
    {
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Rotation X : " + rotation.x.ToString() + "; Rotation Y : " + rotation.y.ToString() + "; Rotation Z : " + rotation.z.ToString());
    }
    public static void DebugPositionAndRotation(this Transform transform, string textToDisplay)
    {
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Position X : " + transform.position.x.ToString() + "; Position Y : " + transform.position.y.ToString() + "; Position Z : " + transform.position.z.ToString());
        Debug.Log("SnippetCode : " + textToDisplay + " : " + "Rotation X : " + transform.rotation.x.ToString() + "; Rotation Y : " + transform.rotation.y.ToString() + "; Rotation Z : " + transform.rotation.z.ToString());
    }

    private static IEnumerator LerpMovement(this Vector3 positionToReach, Quaternion rotationToReach, Item itemToMove, float durationOfMvt)
    {
        foreach (ColliderGroup colliderGroup in itemToMove.colliderGroups)
        {
            foreach (Collider collider in colliderGroup.colliders)
            {
                collider.enabled = false;
            }
        }
        float time = 0;
        Vector3 positionOrigin = itemToMove.transform.position;
        Quaternion orientationOrigin = itemToMove.transform.rotation;
        if (positionToReach != positionOrigin)
        {
            while (time < durationOfMvt)
            {
                //itemToMove.isFlying = true;
                //itemToMove.rb.position = Vector3.Lerp(positionOrigin, positionToReach, time / durationOfMvt);
                //itemToMove.rb.rotation = Quaternion.Lerp(orientationOrigin, rotationToReach, time / durationOfMvt);
                itemToMove.transform.position = Vector3.Lerp(positionOrigin, positionToReach, time / durationOfMvt);
                itemToMove.transform.rotation = Quaternion.Lerp(orientationOrigin, rotationToReach, time / durationOfMvt);
                time += Time.deltaTime;
                yield return null;
            }
        }
        //itemToMove.rb.position = positionToReach;
        foreach (ColliderGroup colliderGroup in itemToMove.colliderGroups)
        {
            foreach (Collider collider in colliderGroup.colliders)
            {
                collider.enabled = true;
            }
        }
    }

    public static List<Item> GetItemsOnCreature(this Creature creature, ItemData.Type? dataType)
    {
        List<Item> list = new List<Item>();
        foreach (Holder holder in creature.equipment.holders)
        {
            foreach (Item item in holder.items)
            {
                if (dataType.HasValue)
                {
                    if (item.data.type == dataType && dataType.HasValue)
                    {
                        list.Add(item);
                    }
                }
            }
        }
        if (creature.handLeft.grabbedHandle?.item != null)
        {
            list.Add(creature.handLeft.grabbedHandle.item);
        }
        if (creature.handRight.grabbedHandle?.item != null)
        {
            list.Add(creature.handRight.grabbedHandle.item);
        }
        if (creature.mana.casterLeft.telekinesis.catchedHandle?.item != null)
        {
            list.Add(creature.mana.casterLeft.telekinesis.catchedHandle?.item);
        }
        if (creature.mana.casterRight.telekinesis.catchedHandle?.item != null)
        {
            list.Add(creature.mana.casterRight.telekinesis.catchedHandle?.item);
        }
        return list;
    }
    public static IEnumerable<Item> ItemsInRadiusAroundItem(this Vector3 position, Item thisItem, float radius)
    {
        return Item.allActive.Where(item =>
            ((item.transform.position - position).sqrMagnitude < radius * radius) && !thisItem
        );
    }

    public static IEnumerable<Item> ItemsInRadius(Vector3 position, float radius)
    {
        return Physics.OverlapSphere(position, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            .SelectNotNull(collider => collider.attachedRigidbody?.GetComponent<CollisionHandler>()?.item)
            .Distinct();
    }

    public static Item ClosestItemAroundItem(this Item thisItem, float radius)
    {
        float lastRadius = Mathf.Infinity;
        Item lastItem = null;
        float thisRadius;
        foreach (Item item in Item.allActive.Where(itemSelect => itemSelect != thisItem))
        {
            thisRadius = (item.transform.position - thisItem.transform.position).sqrMagnitude;
            if (thisRadius < radius * radius && thisRadius < lastRadius)
            {
                lastRadius = thisRadius;
                lastItem = item;
            }
        }
        return lastItem;
    }

    public static Item ClosestItemAroundItemOverlapSphere(this Item thisItem, float radius)
    {
        float lastRadius = Mathf.Infinity;
        Collider lastCollider = null;
        float thisRadius;
        List<Collider> colliders = Physics.OverlapSphere(thisItem.transform.position, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            .Distinct().Where(coll => coll.attachedRigidbody?.GetComponent<CollisionHandler>()?.item != null && coll.attachedRigidbody?.GetComponent<CollisionHandler>()?.item != thisItem).ToList();
        foreach (Collider collider in colliders)
        {
            thisRadius = (collider.ClosestPoint(thisItem.transform.position) - thisItem.transform.position).sqrMagnitude;
            if (thisRadius < radius * radius && thisRadius < lastRadius)
            {
                lastRadius = thisRadius;
                lastCollider = collider;
            }
        }
        if (lastCollider?.attachedRigidbody?.GetComponent<CollisionHandler>().item == null)
        {
            return thisItem;
        }
        else
        {
            return lastCollider.attachedRigidbody.GetComponent<CollisionHandler>().item;
        }
    }
    public static RagdollPart ClosestRagdollMinusPlayerAroundItemOverlapSphere(this Item thisItem, float radius)
    {
        float lastRadius = Mathf.Infinity;
        Collider lastCollider = null;
        float thisRadius;
        List<Collider> colliders = Physics.OverlapSphere(thisItem.transform.position, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            .Distinct().Where(coll => coll.attachedRigidbody?.GetComponent<CollisionHandler>()?.ragdollPart != null && !Player.local.creature.ragdoll.parts.Contains(coll.attachedRigidbody?.GetComponent<CollisionHandler>()?.ragdollPart)).ToList();
        foreach (Collider collider in colliders)
        {
            thisRadius = (collider.ClosestPoint(thisItem.transform.position) - thisItem.transform.position).sqrMagnitude;
            if (thisRadius < radius * radius && thisRadius < lastRadius)
            {
                lastRadius = thisRadius;
                lastCollider = collider;
            }
        }
        if (lastCollider?.attachedRigidbody?.GetComponent<RagdollPart>() == null)
        {
            return thisItem.handlers.First(hand => hand == Player.local.creature.handLeft || hand == Player.local.creature.handRight);
        }
        else
        {
            return lastCollider.attachedRigidbody.GetComponent<RagdollPart>();
        }
    }

    public static Creature FarestCreatureInAConeRaycast(this Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle, int layer = Physics.DefaultRaycastLayers)
    {
        float lastRadius = 0f;
        float thisRadius;
        RagdollPart lastRagdollPart = null;
        RaycastHit[] raycastHits = ConeCastAll(origin, maxRadius, direction, maxDistance, coneAngle, layer);
        List<RagdollPart> ragdollPartsHit = new List<RagdollPart>();
        foreach (RaycastHit raycastHit in raycastHits)
        {
            if (raycastHit.rigidbody?.gameObject.GetComponent<RagdollPart>() is RagdollPart)
            {
                if (raycastHit.rigidbody.gameObject.GetComponent<RagdollPart>() is RagdollPart ragdollPart && !ragdollPartsHit.Contains(ragdollPart) && !ragdollPart.isSliced && !ragdollPart.ragdoll.creature.isKilled && !ragdollPart.ragdoll.creature.isPlayer)
                {
                    ragdollPartsHit.Add(ragdollPart);
                }
            }
        }
        foreach (RagdollPart ragdollPart1 in ragdollPartsHit)
        {
            thisRadius = (ragdollPart1.transform.position - origin).sqrMagnitude;
            if(thisRadius > lastRadius)
            {
                lastRadius = thisRadius;
                lastRagdollPart = ragdollPart1;
            }
        }
        if(lastRagdollPart != null)
        {
            return lastRagdollPart.ragdoll.creature;
        }
        else
        {
            return null;
        }
    }
    public static Creature FarestCreatureInAConeRaycastLocomotion(this Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle)
    {
        float lastRadius = 0f;
        float thisRadius;
        Creature lastCreature = null;
        RaycastHit[] raycastHits = ConeCastAll(origin, maxRadius, direction, maxDistance, coneAngle, LayerMask.GetMask("BodyLocomotion"));
        List<Creature> creaturesHit = new List<Creature>();
        foreach (RaycastHit raycastHit in raycastHits)
        {
            if (raycastHit.collider?.GetComponentInParent<Creature>() is Creature creature && !creaturesHit.Contains(creature) && !creature.isKilled && !creature.isPlayer)
            {
                creaturesHit.Add(creature);
            }
        }
        foreach (Creature creature in creaturesHit)
        {
            thisRadius = (creature.transform.position - origin).sqrMagnitude;
            if(thisRadius > lastRadius)
            {
                lastRadius = thisRadius;
                lastCreature = creature;
            }
        }
        if(lastCreature != null)
        {
            return lastCreature;
        }
        else
        {
            return null;
        }
    }


    public static int ReturnNbFreeSlotOnCreature(this Creature creature)
    {
        int nbFreeSlots = 0;
        foreach (Holder holder in creature.equipment.holders)
        {
            if (holder.currentQuantity != 0)
            {
                nbFreeSlots++;
            }
        }
        return nbFreeSlots;
    }

    public static IEnumerable<Component> GetComponentsOfGameObject(this GameObject gameObject, bool allInactive)
    {
        return allInactive ? gameObject.GetComponents(typeof(Component)) : gameObject.GetComponents(typeof(Component)).Where(component => component.gameObject.activeSelf);
    }

    public static void listAllComponentsOfGameObject(this GameObject gameObject, bool allInactive = true)
    {
        int i = 0;
        foreach (Component component in GetComponentsOfGameObject(gameObject, allInactive))
        {
            Debug.Log("Component " + i + " of " + component.name + " : " + component.ToString() + " : " + component.gameObject.name);
            i++;
        }
    }

    public static Vector3 HomingTarget(Rigidbody projectileRb, Vector3 targetPosition, float initialDistance, float forceFactor)
    {
        return Vector3.Lerp(projectileRb.velocity, (targetPosition - projectileRb.position).normalized * Vector3.Distance(targetPosition, projectileRb.position) * forceFactor, Vector3.Distance(targetPosition, projectileRb.position).Remap01(initialDistance, 2f));
    }


    public static float PingPongValue(float min, float max, float speed)
    {
        return Mathf.Lerp(min, max, Mathf.PingPong(Time.time * speed, 1));
    }


    public class SlowBehaviour : MonoBehaviour
    {
        private Creature creature;
        private string brainId;
        private float orgAnimatorSpeed;
        private float orgLocomotionSpeed;
        private bool hasStarted = false;
        private bool isSlowed = false;
        private bool endOfSlow = false;
        private float timerStart;
        private float orgTimerStart;
        private float timerDuration;
        private float orgTimerDuration;
        private float timerBlend;
        private float orgTimerBlend;
        private float ratioSlow;
        private float orgRatioSlow;
        private bool playVFX;
        private bool restoreVelocity;
        private Vector3 orgCreatureVelocity;
        private Vector3 orgCreatureAngularVelocity;
        private List<Vector3> orgCreatureVelocityPart;
        private List<Vector3> orgCreatureAngularVelocityPart;
        private List<float> orgCreatureDragPart;
        private List<float> orgCreatureAngularDragPart;
        private float orgLocomotionDrag;
        private float orgLocomotionAngularDrag;
        private float factor = 10f;

        public void Init(float start, float duration, float ratio, bool restoreVelocityAfterEffect = true, float blendDuration = 0f, bool playEffect = false)
        {
            timerStart = start;
            orgTimerStart = start;
            timerDuration = duration;
            orgTimerDuration = duration;
            ratioSlow = ratio;
            orgRatioSlow = ratio;
            timerBlend = blendDuration;
            orgTimerBlend = blendDuration;
            playVFX = playEffect;
            restoreVelocity = restoreVelocityAfterEffect;
        }

        public void Awake()
        {
            creature = GetComponent<Creature>();
            creature.OnDespawnEvent += time => Dispose();
            creature.OnKillEvent += Creature_OnKillEvent;
        }

        private void Creature_OnKillEvent(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                Dispose();
            }
        }

        public void Update()
        {
            // Wait for the start
            if (hasStarted != true)
            {
                timerStart -= Time.deltaTime;
                timerStart = Mathf.Clamp(timerStart, 0, orgTimerStart);
                if (timerStart <= 0.0f)
                {
                    brainId = creature.ragdoll.creature.brain.instance.id;
                    orgAnimatorSpeed = creature.animator.speed;
                    orgLocomotionSpeed = creature.locomotion.speed;
                    orgCreatureVelocity = creature.locomotion.rb.velocity;
                    orgCreatureAngularVelocity = creature.locomotion.rb.angularVelocity;
                    orgCreatureVelocityPart = creature.ragdoll.parts.Select(part => part.rb.velocity).ToList();
                    orgCreatureAngularVelocityPart = creature.ragdoll.parts.Select(part => part.rb.angularVelocity).ToList();
                    orgCreatureDragPart = creature.ragdoll.parts.Select(part => part.rb.drag).ToList();
                    orgCreatureAngularDragPart = creature.ragdoll.parts.Select(part => part.rb.angularDrag).ToList();
                    orgLocomotionDrag = creature.locomotion.rb.drag;
                    orgLocomotionAngularDrag = creature.locomotion.rb.angularDrag;
                    hasStarted = true;
                }
            }

            // Slow is blended
            if (hasStarted == true && isSlowed != true)
            {
                if (orgTimerBlend != 0f)
                {
                    timerBlend -= Time.deltaTime / orgTimerBlend;
                    timerBlend = Mathf.Clamp(timerBlend, 0, orgTimerBlend);
                }
                else
                {
                    timerBlend = 0f;
                }

                creature.animator.speed = Mathf.Lerp(orgAnimatorSpeed * ratioSlow / factor, orgAnimatorSpeed, timerBlend);
                creature.locomotion.speed = Mathf.Lerp(orgAnimatorSpeed * ratioSlow / factor, orgLocomotionSpeed, timerBlend);
                creature.locomotion.rb.velocity = new Vector3(Mathf.Lerp(orgCreatureVelocity.x * ratioSlow / factor, orgCreatureVelocity.x, timerBlend),
                                                            Mathf.Lerp(orgCreatureVelocity.y * ratioSlow / factor, orgCreatureVelocity.y, timerBlend),
                                                            Mathf.Lerp(orgCreatureVelocity.z * ratioSlow / factor, orgCreatureVelocity.z, timerBlend));
                creature.locomotion.rb.angularVelocity = new Vector3(Mathf.Lerp(orgCreatureAngularVelocity.x * ratioSlow / factor, orgCreatureAngularVelocity.x, timerBlend),
                                                                    Mathf.Lerp(orgCreatureAngularVelocity.y * ratioSlow / factor, orgCreatureAngularVelocity.y, timerBlend),
                                                                    Mathf.Lerp(orgCreatureAngularVelocity.z * ratioSlow / factor, orgCreatureAngularVelocity.z, timerBlend));
                creature.locomotion.rb.drag = Mathf.Lerp(factor * 100f, orgLocomotionDrag, timerBlend);
                creature.locomotion.rb.angularDrag = Mathf.Lerp(factor * 100f, orgLocomotionAngularDrag, timerBlend);
                for (int i = creature.ragdoll.parts.Count() - 1; i >= 0; --i)
                {
                    creature.ragdoll.parts[i].ragdoll.SetPhysicModifier(this, 5, 0, 0, factor * 100f, factor * 100f);
                    creature.ragdoll.parts[i].rb.velocity = new Vector3(Mathf.Lerp(orgCreatureVelocityPart[i].x * ratioSlow / factor, orgCreatureVelocityPart[i].x, timerBlend),
                                                                        Mathf.Lerp(orgCreatureVelocityPart[i].x * ratioSlow / factor, orgCreatureVelocityPart[i].y, timerBlend),
                                                                        Mathf.Lerp(orgCreatureVelocityPart[i].x * ratioSlow / factor, orgCreatureVelocityPart[i].z, timerBlend));
                    creature.ragdoll.parts[i].rb.angularVelocity = new Vector3(Mathf.Lerp(orgCreatureAngularVelocityPart[i].x * ratioSlow / factor, orgCreatureAngularVelocityPart[i].x, timerBlend),
                                                                                Mathf.Lerp(orgCreatureAngularVelocityPart[i].y * ratioSlow / factor, orgCreatureAngularVelocityPart[i].y, timerBlend),
                                                                                Mathf.Lerp(orgCreatureAngularVelocityPart[i].z * ratioSlow / factor, orgCreatureAngularVelocityPart[i].z, timerBlend));
                    creature.ragdoll.parts[i].rb.drag = Mathf.Lerp(factor * 100f, orgCreatureDragPart[i], timerBlend);
                    creature.ragdoll.parts[i].rb.angularDrag = Mathf.Lerp(factor * 100f, orgCreatureAngularDragPart[i], timerBlend);
                }

                if (timerBlend <= 0.0f)
                {
                    isSlowed = true;
                    creature.GetPart(RagdollPart.Type.Torso).rb.freezeRotation = true;
                    Debug.Log("Drag : " + creature.ragdoll.parts[0].rb.drag);
                    //creature.brain.Stop();
                    //creature.brain.StopAllCoroutines();
                    //creature.locomotion.MoveStop();
                    //creature.StopAnimation();
                }
            }

            // Slow is active and wait for the end of the duration
            if (isSlowed == true && endOfSlow != true)
            {
                timerDuration = Mathf.Clamp(timerDuration, 0, orgTimerDuration);
                timerDuration -= Time.deltaTime;
                if (timerDuration <= 0.0f)
                {
                    endOfSlow = true;
                }
            }
            if (endOfSlow == true)
            {
                Dispose();
            }
        }
        public void Dispose()
        {
            if(creature != null)
            {
                creature.animator.speed = orgAnimatorSpeed;
                creature.locomotion.speed = orgLocomotionSpeed;
                foreach (RagdollPart ragdollPart in creature.ragdoll.parts)
                {
                    ragdollPart.ragdoll.RemovePhysicModifier(this);
                }
                creature.GetPart(RagdollPart.Type.Torso).rb.freezeRotation = false;
                if (restoreVelocity)
                {
                    creature.locomotion.rb.velocity = orgCreatureVelocity;
                    creature.locomotion.rb.angularVelocity = orgCreatureAngularVelocity;
                    creature.locomotion.rb.drag = orgLocomotionDrag;
                    creature.locomotion.rb.angularDrag = orgLocomotionAngularDrag;
                    for (int i = creature.ragdoll.parts.Count() - 1; i >= 0; --i)
                    {
                        creature.ragdoll.parts[i].rb.velocity = orgCreatureVelocityPart[i];
                        creature.ragdoll.parts[i].rb.angularVelocity = orgCreatureAngularVelocityPart[i];
                        creature.ragdoll.parts[i].rb.drag = orgCreatureDragPart[i];
                        creature.ragdoll.parts[i].rb.angularDrag = orgCreatureAngularDragPart[i];
                    }
                }
            }
            //creature.brain.Load(brainId);
            Destroy(this);
        }
    }
}