using UnityEngine;

public class enemyCarAttack : MonoBehaviour
{
    public EnemyCarControl Car;
    public void attackDone(string message)
    {
        if (message.Equals("HammerSwung"))
        {
            Car.HammerAttack();
        }
        if (message.Equals("SwordOne"))
        {
            Car.SwordAttackOne();
        }
        if (message.Equals("SwordTwo"))
        {
            Car.SwordAttackTwo();
        }
        if (message.Equals("KnifeOne"))
        {
            Car.KnifeAttackOne();
        }
        if (message.Equals("KnifeTwo"))
        {
            Car.KnifeAttackTwo();
        }
        if (message.Equals("ArrowShot"))
        {
            Car.ArrowShot();
        }
    }
}
