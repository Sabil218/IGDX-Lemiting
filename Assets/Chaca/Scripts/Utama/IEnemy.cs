public interface IEnemy
{
    bool IsDead { get; }

    void Attack();

    void TakeDamage(int amount);
}