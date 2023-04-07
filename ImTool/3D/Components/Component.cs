using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace ImTool.Scene3D
{
    public class Component
    {
        public string Name;
        public Actor Owner;
        public ActorFlags Flags;
        public Transform Transform;

        public virtual void Init(Actor owner)
        {
            Owner = owner;
        }

        public virtual void Update(double dt)
        {

        }

        public virtual void Render(CommandList cmdList)
        {

        }
    }
}
