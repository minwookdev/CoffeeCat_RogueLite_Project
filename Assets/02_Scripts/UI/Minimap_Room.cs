using System;
using System.Collections;
using System.Collections.Generic;
using RandomDungeonWithBluePrint;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat
{
    // TODO : RoomIcon 어드레서블로 변경
    public class Minimap_Room : MonoBehaviour
    {
        // 방 입장시 미니맵에 표시
        // 클리어한 룸인지 표시
        private const string roomIconPath = "UI/RoomIcon";
        private int roomIndex = 0;

        [SerializeField] private RectTransform rectTr = null;
        [SerializeField] private GameObject defaultPanel = null;
        [SerializeField] private GameObject clearPanel = null;
        [SerializeField] private GameObject currentIcon = null;
        [SerializeField] private Image roomIcon = null;

        private void SetRoomIcon(RoomType roomType)
        {
            Sprite icon = null;
            switch (roomType)
            {
                case RoomType.PlayerSpawnRoom:
                    icon = null;
                    break;
                case RoomType.MonsterSpawnRoom:
                    icon = Resources.Load<Sprite>($"{roomIconPath}/Monster");
                    break;
                case RoomType.ShopRoom:
                    icon = Resources.Load<Sprite>($"{roomIconPath}/Shop");
                    break;
                case RoomType.BossRoom:
                    icon = Resources.Load<Sprite>($"{roomIconPath}/Boss");
                    break;
                case RoomType.RewardRoom:
                    icon = Resources.Load<Sprite>($"{roomIconPath}/Reward");
                    break;
                case RoomType.EmptyRoom:
                    icon = null;
                    break;
                case RoomType.ExitRoom:
                    icon = Resources.Load<Sprite>($"{roomIconPath}/Exit");
                    break;
            }

            if (icon == null)
            {
                roomIcon.enabled = false;
                return;
            }

            roomIcon.sprite = icon;
        }

        public void Initialize(Room room, float ratio)
        {
            roomIndex = room.RoomData.RoomIndex;
            rectTr.localScale = Vector3.one;
            rectTr.sizeDelta = new Vector2(room.Rect.width, room.Rect.height) * ratio;
            rectTr.anchoredPosition = new Vector2(room.Rect.x, room.Rect.y) * ratio;
            SetRoomIcon(room.RoomType);
            gameObject.SetActive(false);
        }

        public void EnterdRoom()
        {
            gameObject.SetActive(true);
            currentIcon.SetActive(true);
        }

        public void LeftRoom()
        {
            currentIcon.SetActive(false);
        }

        public void ClearedRoom()
        {
            defaultPanel.SetActive(false);
            clearPanel.SetActive(true);
        }
    }
}