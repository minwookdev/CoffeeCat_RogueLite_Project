using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeCat
{
    public class CameraController : MonoBehaviour
    {
        private Player_Dungeon player;
        [SerializeField] private float test;

        private void Start()
        {
            //player = StageManager.Instance.player.GetComponent<Player>();
        }

        private void Update()
        {
            if (!player)
                return;

            //var targetPos = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
            //targetPos.x = Mathf.Clamp(targetPos.x, StageManager.Instance.CurrentRoom.Rect.xMin, StageManager.Instance.CurrentRoom.Rect.xMax);
            //targetPos.y = Mathf.Clamp(targetPos.y, StageManager.Instance.CurrentRoom.Rect.yMin, StageManager.Instance.CurrentRoom.Rect.yMax);
        }
    }
}
