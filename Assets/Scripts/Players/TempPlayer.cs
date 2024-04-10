using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UnityEngine;

namespace CoffeeCat {
    public class TempPlayer : MonoBehaviour {
        public TempPlayerStat Stats = null;

        private void Start() {
            Stats = new TempPlayerStat();
            Stats.HP = 100;
        }

        public void OnDamaged(in AttackData attackData, Vector2 collisionPoint, Vector2 collisionDirection) {
            //DamageTextManager.Instance.OnFloatingDamageText(attackData.CalculatedDamage, collisionPoint);
            //DamageTextManager.Instance.OnReflectingDamageText(attackData.CalculatedDamage, collisionPoint, collisionDirection);
            DamageTextManager.Instance.OnTransmittanceText(attackData.CalculatedDamage, collisionPoint, collisionDirection);

            //float angle = Mathf.Atan2(-collisionDirection.y, collisionDirection.x) * Mathf.Rad2Deg;
            //Quaternion direcitonGuideRotation = Quaternion.AngleAxis(angle, Vector3.forward);   // <-- Way 1
            //Quaternion direcitonGuideRotation = Quaternion.Euler(new Vector3(0f, 0f, angle)); // <-- Way 2

            //Quaternion direcitonGuideRotation = Math2DHelper.GetRotationByDirection(collisionDirection);
            //ObjectPoolManager.Instance.Spawn("Direction_Guide", collisionPoint, direcitonGuideRotation);

            var healthPointLeft = Stats.HP - attackData.CalculatedDamage;
            if (healthPointLeft <= 0) {
                Stats.HP = 0;
            }
            else {
                Stats.HP = healthPointLeft; 
            }
        }
    }
}

