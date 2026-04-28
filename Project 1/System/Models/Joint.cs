using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.System.Models
{
    internal abstract class Joint 
    {
        public void AddChild(Joint child) 
        {
            child.parent = this;
            children.Add(child);
        }
        public Joint Parent => parent;
        Joint parent;
        List<Joint> children;

        protected Joint(Joint aParent)
        {
            parent = aParent;
            if (parent != null) parent.AddChild(this);
            children = new List<Joint>();
        }

        public virtual void Update()
        {
            foreach (Joint child in children)
            {
                child.Update();
            }
        }
    }
}
