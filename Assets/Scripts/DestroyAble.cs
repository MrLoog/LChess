using System;

namespace Assets.Scripts
{
    public interface DestroyAble
    {
        void Attach(Action<object> action);

        void Detach(Action<object> action);

        void NotifyAllObserver();
    }

    public interface ObserverDestroy
    {
        void OnTargetDestroy();
    }
}
