using UnityEngine;
using DG.Tweening;
using TMPro;
using CoffeeCat.Utils;
using CoffeeCat.FrameWork;

namespace CoffeeCat.UI {
    public class DamageText : MonoBehaviour {
        // FIELDS
        public Transform Tr { get; private set; }
        private TextMeshPro damageTmp = null;
        private Color originColor = Color.white;
        private readonly float defaultMoveDistance = 1.5f;
        
        // 1. Change Damage Text ui -> world GameObject
        // 2. SetAsSibling

        private void Awake() {
            Tr = GetComponent<Transform>();
            damageTmp = GetComponent<TextMeshPro>();
            originColor = damageTmp.color;
            damageTmp.SetColorZero();
        }

        public void OnFloating(Vector3 startPosition, string text) {
            Tr.position = startPosition;
            Tr.DOMoveY(Tr.position.y + defaultMoveDistance, 1f)
              .OnStart(() => {
                  damageTmp.SetText(text);
                  damageTmp.color = originColor;
              })
              .OnComplete(() => {
                  ObjectPoolManager.Instance.Despawn(this.gameObject);
              });
        }

        public void OnReflecting(Vector2 startPosition, Vector2 direction, string text) {
            Tr.position = startPosition;
            //Vector2 destinationPoint = (Vector2)Tr.position + Math2DHelper.GetInverseVector(direction * defaultMoveDistance);
            Vector2 destinationPoint = Math2DHelper.GetInversePointByDirection(Tr.position, direction, defaultMoveDistance);
            Tr.DOMove(destinationPoint, 1.25f)
              .OnStart(() => {
                  damageTmp.SetText(text);
                  damageTmp.color = originColor;
              })
              .OnComplete(() => {
                  ObjectPoolManager.Instance.Despawn(this.gameObject);
              });
        }

        public void OnTransmittance(Vector2 startPosition, Vector2 direction, string text) {
            Tr.position = startPosition;
            //Vector2 destPosition = startPosition + (direction * defaultMoveDistance);
            Vector2 destinationPoint = Math2DHelper.GetPointByDirection(Tr.position, direction, defaultMoveDistance);
            Tr.DOMove(destinationPoint, 1f)
                .OnStart(() => {
                    damageTmp.SetText(text);
                    damageTmp.color = originColor;
                })
                .OnComplete(() => {
                    ObjectPoolManager.Instance.Despawn(this.gameObject);
                });
        }
    }
}
