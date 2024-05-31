using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.UI;

namespace CoffeeCat.FrameWork {
    public class UIPresenter : PrelocatedSingleton<UIPresenter> {
        [Title("UI")]
        [SerializeField] private SkillSelectPanel skillSelector = null;
        
        public void OpenSkillSelectPanel(PlayerSkillSelectData[] datas) {
            skillSelector.Open(datas);
        }
    }
}
