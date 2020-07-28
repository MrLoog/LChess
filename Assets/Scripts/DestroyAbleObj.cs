using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class DestroyAbleObj : DestroyAble
    {
        public object Source { get; set; }
        public DestroyAbleObj(object source)
        {
            Source = source;
        }
        List<ObserverDestroy> observerDestroys = new List<ObserverDestroy>();
        List<Action<object>> Actions = new List<Action<object>>();

        public void Attach(ObserverDestroy observer)
        {
            observerDestroys.Add(observer);
        }

        public void Attach(Action<object> action)
        {
            Actions.Add(action);
        }

        public void Detach(ObserverDestroy observer)
        {
            observerDestroys.Remove(observer);
        }

        public void Detach(Action<object> action)
        {
            Actions.Remove(action);
        }

        public void NotifyAllObserver()
        {
            for (int i = 0; i < observerDestroys.Count; i++)
            {
                observerDestroys[i].OnTargetDestroy();
            }
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i](Source);
            }
        }
    }
}
