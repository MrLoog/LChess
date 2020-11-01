namespace Assets.Scripts
{
    public interface ReceivedDmgAble
    {
        float GetCurrentHealth();
        void SetCurrentHealth(float health);
        void ApplyDamage(float damage);
    }
}
