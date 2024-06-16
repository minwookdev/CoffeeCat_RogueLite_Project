using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.UI {
    public class SkillSelectPanel : MonoBehaviour {
        [SerializeField] private SkillSelectButton[] selectButtons = null;

        private void Start() {
            StageManager.Inst.AddEventToSkillSelectCompleted(() => {
                gameObject.SetActive(false);
            });
        }

        public void Open(PlayerSkillSelectData[] datas) {
            if (gameObject.activeSelf) {
                CatLog.WLog("Select Panel is Already Opened");
                return;
            }
            
            if(datas is not { Length: Defines.PLAYER_SKILL_SELECT_COUNT }) {
                CatLog.WLog("Panel Open Failed: SkillSelectPanel.Set() - Invalid Data Length");
                return;
            }
            
            // Setup Skill Select Buttons
            for (int i = 0; i < Defines.PLAYER_SKILL_SELECT_COUNT; i++) {
                var data = datas[i];
                if (data == null) {
                    CatLog.WLog("Data is Null");
                    return;
                }
                
                selectButtons[i].Set(data);
            }
            
            StageManager.Inst.InvokeOpeningSkillSelectPanel();
            gameObject.SetActive(true);
        }

        public void ClosePanel() {
            gameObject.SetActive(false);
            for (int i = 0; i < selectButtons.Length; i++) {
                selectButtons[i].Clear();
            }
        }
    }
}
