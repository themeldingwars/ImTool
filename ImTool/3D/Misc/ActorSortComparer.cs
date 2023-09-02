using ImTool.Scene3D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImTool.Scene3D
{
    public class ActorSortComparer : IComparer<Actor>
    {
        public Vector3 CameraPosition;

        public int Compare(Actor actor1, Actor actor2)
        {
            var actor1Dist = Vector3.Distance(actor1.Transform.Position,  CameraPosition) * actor1.RenderOrderBoost;
            var actor2Dist = Vector3.Distance(actor2.Transform.Position, CameraPosition) * actor2.RenderOrderBoost;

            if (actor1Dist == actor2Dist)
            {
                return 0;
            }
            else if (actor1Dist > actor2Dist)
            {
                return 1;
            }
            else if (actor1Dist < actor2Dist)
            {
                return -1;
            }

            return 0;
        }
    }
}
