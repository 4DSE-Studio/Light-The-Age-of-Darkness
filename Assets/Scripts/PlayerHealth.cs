using Legacy;

public class PlayerHealth : BasicHealth
{
    protected override void ApplyDamage(float damage, Power type)
    {
        if (type == Power.Light)
            Heal(damage);
        else
            base.ApplyDamage(damage, type);
    }
}