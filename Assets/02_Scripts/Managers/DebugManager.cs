namespace CoffeeCat.Editor {
#if UNITY_EDITOR || DEBUG_MODE
    using System;
    using UnityEngine;
    using FrameWork;
    
    /// <summary>
    /// DebugManager GameObject attaches 'EditorOnly' Tag.
    /// </summary>
    public class DebugManager : SceneSingleton<DebugManager> {
        private void Update() {
            // Enable Skill Panel
            if (Input.GetKeyDown(KeyCode.O)) {
                var player = RogueLiteManager.Inst.SpawnedPlayer;
                if (player) {
                    player.EnableSkillSelect();
                }
            }

            // Enable Skill Information Panel
            if (Input.GetKeyDown(KeyCode.P)) {
                DungeonUIPresenter.Inst.OpenPlayerSkillsPanel();
            }

            // Enable Forced Next Floor
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (StageManager.IsExist) {
                    StageManager.Inst.RequestGenerateNextFloor();
                }
            }
        }
    }
#endif
}