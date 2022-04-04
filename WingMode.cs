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
        private float rotation;
        private int partPicked = 6;
        private List<int> nbMainWing = new List<int> { 1, 2, 3, 9, 10, 11, 15 };
        private List<int> nbOtherWing = new List<int> { 4, 5, 7, 8, 12, 13, 14 };
        private float spanOfWing = 1.0f;

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
        }

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            if (nbMainWing.Contains(index))
            {
                if (nbMainWing.IndexOf(index) == 0)
                {
                    return Center() + ForwardDir() * spanOfWing / 3f;
                }
                else if (nbMainWing.IndexOf(index) == 1 || nbMainWing.IndexOf(index) == 2)
                {
                    return Center() + ForwardDir() * spanOfWing / 2f;
                }
                else
                {
                    return Center() + ForwardDir() * spanOfWing / 1f;
                }
            }
            else
            {
                if (nbOtherWing.IndexOf(index) == 0)
                {
                    return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing / 3f;
                }
                else if (nbOtherWing.IndexOf(index) == 1 || nbOtherWing.IndexOf(index) == 2)
                {
                    return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing / 2f;
                }
                else
                {
                    return CenterOtherHand() + ForwardDirOtherHand() * spanOfWing / 1f;
                }
            }
        }

        public override Quaternion GetRot(int index, Rigidbody rb, BladePart part)
        {
            if (nbMainWing.Contains(index))
            {
                return Quaternion.LookRotation(ForwardDir(), rb.transform.position - Center());
            }
            else
            {
                return Quaternion.LookRotation(ForwardDirOtherHand(), rb.transform.position - CenterOtherHand());
            }
        }

        public override void Update()
        {
            base.Update();
            foreach(BladePart parts in sword.parts)
            {
                if(parts.item.rb.velocity.sqrMagnitude > 50f && Vector3.SignedAngle(Player.local.creature.transform.forward,Vector3.Cross(parts.item.rb.velocity, Player.local.creature.transform.right), Player.local.transform.forward) < 90f)
                {
                    //Player.local.locomotion.rb.AddForce(Vector3.ProjectOnPlane(parts.item.rb.velocity * 1f, Player.local.creature.transform.right), ForceMode.Impulse);
                    // use Dot product for the 
                    Player.local.locomotion.rb.AddForce(-parts.item.rb.velocity * 1f, ForceMode.Impulse);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
