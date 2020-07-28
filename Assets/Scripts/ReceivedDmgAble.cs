using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public interface ReceivedDmgAble
    {
        float GetCurrentHealth();
        void SetCurrentHealth(float health);
    }
}
