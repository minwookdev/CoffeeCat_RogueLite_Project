using System;
using CoffeeCat;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CoffeeCat.FrameWork;
using UnityEngine.Events;

namespace CoffeCat.UI {
    public class SkillSelectButton : MonoBehaviour {
        [SerializeField] private Button button = null;
        [SerializeField] private TextMeshProUGUI tmpName = null;
        [SerializeField] private TextMeshProUGUI tmpDesc = null;
        [SerializeField] private TextMeshProUGUI tmpType = null;
        [SerializeField] private Image imgIcon = null;
        [SerializeField] private int index = -1;

        private void Start() {
            // button.onClick.RemoveAllListeners();
            AddButtonEvent(() => {
                if (!RogueLiteManager.IsExist) {
                    return;
                }
                var player = RogueLiteManager.Instance.SpawnedPlayer;
                if (!player) {
                    return;
                }

                player.UpdateSkill(index);
                StageManager.Instance.InvokeSkillSelectCompleted();
            });
        }

        /*public void AddDisableButtonEvent(GameObject parentPanel) {
            AddButtonEvent(() => {
                parentPanel.SetActive(false);
            });
        }*/

        public void Set(PlayerSkillSelectData data) {
            tmpName.SetText(data.Name);
            tmpDesc.SetText(data.Desc);
            tmpType.SetText(data.Type == 0 ? "<< Passive >>" : "<< Active >>");
            if (data.Icon) {
                imgIcon.sprite = data.Icon;   
            }
            index = data.Index;
        }

        public void Clear() {
            tmpName.SetText("");
            tmpDesc.SetText("");
            tmpType.SetText("");
            /*imgIcon.sprite = null;*/
            index = -1;
        }

        private void AddButtonEvent(UnityAction unityAction) {
            button.onClick.AddListener(unityAction);
        }
    }
}
