using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class DmgCalculate
    {
        private ReceivedDmgAble received;
        private DealDmgAble dealer;

        public DmgCalculate(ReceivedDmgAble received, DealDmgAble dealer)
        {
            this.received = received;
            this.dealer = dealer;
        }

        public float PerformDamage()
        {
            float damage = dealer.GetDamage();
            received.SetCurrentHealth(received.GetCurrentHealth() - damage);
            return damage;
        }
        
    }
}
