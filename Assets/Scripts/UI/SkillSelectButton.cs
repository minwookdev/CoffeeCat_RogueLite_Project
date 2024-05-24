using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CoffeeCat.FrameWork;

namespace CoffeCat.UI {
    public class SkillSelectButton : MonoBehaviour {
        [SerializeField] private Button button = null;
        [SerializeField] private TextMeshProUGUI tmpName = null;
        [SerializeField] private TextMeshProUGUI tmpDesc = null;
        [SerializeField] private TextMeshProUGUI tmpType = null;
        [SerializeField] private Image imgIcon = null;
        [SerializeField] private int index = 0;

        private void Start() {
            button.onClick.AddListener(() => {
                if (!RogueLiteManager.IsExist) {
                    return;
                }
                var player = RogueLiteManager.Instance.SpawnedPlayer;
                if (!player) {
                    return;
                }

                player.UpdateSkill(index);
            });
        }

        public void SetButton(PlayerSkillSelectData data) {
            tmpName.SetText(data.Name);
            tmpDesc.SetText(data.Desc);
            if (data.Icon) {
                imgIcon.sprite = data.Icon;   
            }
            index = data.Index;
        }
    }
}
