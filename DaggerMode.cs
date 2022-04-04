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
using DaggerBending;

namespace ShatterOrb
{
    public class DaggerMode : SpellMode<SpellDagger>
    {
        private float rotation;
        private int partPicked = 6;


        public override void OnItemLoaded(Item item) { base.OnItemLoaded(item); }
        public override int TargetPartNum() => partPicked;

        public override void Enter(Shatterblade.Shatterblade sword)
        {
            base.Enter(sword);
        }

        public override Vector3 GetPos(int index, Rigidbody rb, BladePart part)
        {
            return Center();
        }

        public override Quaternion GetRot(int index, Rigidbody rb, BladePart part)
        {
            return Quaternion.LookRotation(ForwardDir(), rb.transform.position - Center());
        }
    }
}
