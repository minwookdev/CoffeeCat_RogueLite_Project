using System;
using CoffeeCat;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CoffeeCat.FrameWork;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace CoffeCat.UI {
    public class SkillSelectButton : MonoBehaviour {
        [SerializeField] private Button button = null;
        [SerializeField] private TextMeshProUGUI tmpName = null;
        [SerializeField] private TextMeshProUGUI tmpDesc = null;
        [SerializeField] private TextMeshProUGUI tmpType = null;
        [SerializeField] private Image imgIcon = null;
        [SerializeField, ReadOnly] private PlayerSkillSelectData data = null;

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

                player.UpdateSkill(data);
                StageManager.Instance.InvokeSkillSelectCompleted();
            });
        }

        /*public void AddDisableButtonEvent(GameObject parentPanel) {
            AddButtonEvent(() => {
                parentPanel.SetActive(false);
            });
        }*/

        public void Set(PlayerSkillSelectData recievedData) {
            tmpName.SetText(recievedData.Name);
            tmpDesc.SetText(recievedData.Desc);
            tmpType.SetText(recievedData.Type == 0 ? "<< Passive >>" : "<< Active >>");
            if (recievedData.Icon) {
                imgIcon.sprite = recievedData.Icon;   
            }
            data = recievedData;
        }

        public void Clear() {
            tmpName.SetText("");
            tmpDesc.SetText("");
            tmpType.SetText("");
            /*imgIcon.sprite = null;*/ 
            data = null;
        }

        private void AddButtonEvent(UnityAction unityAction) {
            button.onClick.AddListener(unityAction);
        }
    }
}
