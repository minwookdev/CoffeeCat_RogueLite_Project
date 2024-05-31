/// CODER	      :		
/// MODIFIED DATE : 
/// IMPLEMENTATION: 
using UnityEngine;
using CoffeeCat.Utils;

/// NOTE: 벡터의 내적 외적 테스트 스크립트
///
namespace CoffeeCat {
    public class InnerDotProductTest : MonoBehaviour {
        public GameObject sphereGameObject = null;

        public Transform targetTr = null;
        private Transform thisTr = null;

        private void Start() {
            thisTr = GetComponent<Transform>();
        }

        private void Update() {
            //MathHelper.IsTargetInFront(thisTr, targetTr, thisTr.right);

            if (Input.GetKeyDown(KeyCode.M)) {
                //Vector2 direction = Math2DHelper.GetNormalizedDirection(thisTr.position, targetTr.position);
                //Vector2 targetPos = Math2DHelper.GetPointByDirection(thisTr.position, direction, 2f);
                //thisTr.position = targetPos;

                //Vector2 direction2 = (Vector2)(targetTr.position - thisTr.position).normalized;
                //Vector2 targetPos2 = (Vector2)thisTr.position + (direction2 * 2f);
                //thisTr.position = targetPos2;

                Vector2 direction = Math2DHelper.GetNormalizedDirection(thisTr.position, targetTr.position);
                for (int i = 0; i < 20; i++) {
                    Vector2 spawnPosition = Math2DHelper.GetPointByDirection(thisTr.position, direction, 5f * i);
                    Instantiate(sphereGameObject, spawnPosition, Quaternion.identity);
                }
            }
        }
    }
}
