using Legacy;

public class EnemyHealth : BasicHealth
{
    protected override void ApplyDamage(float damage, Power type)
    {
        if (type == Power.Dark)
            Heal(damage);
        else
            base.ApplyDamage(damage, type);
    }
}