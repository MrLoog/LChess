﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Component
{
    public abstract class LComponent
    {
        public virtual bool Update()
        {
            return true;
        }
    }
}
