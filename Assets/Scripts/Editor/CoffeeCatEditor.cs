using System;
using UnityEditor;
using UnityEngine;

namespace CoffeeCat.Editor {
    public class CoffeeCatEditor : UnityEditor.Editor {
        [MenuItem("CoffeeCat/Remove PlayerPrefs", false, 2)]
        public static void RemoveAllPlayerPrefs() {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("CoffeeCat/Captrue GameView/1X")]
        public static void Capture1XScreenShot() {
            CaptureGameView(1);
        }
        
        [MenuItem("CoffeeCat/Captrue GameView/2X")]
        public static void Capture2XScreenShot() {
            CaptureGameView(2);
        }
        
        [MenuItem("CoffeeCat/Captrue GameView/3X")]
        public static void Capture3XScreenShot() {
            CaptureGameView(3);
        }

        private static void CaptureGameView(int size) {
            string imgName = "IMG-" + DateTime.Now.Year.ToString() +
                             DateTime.Now.Month.ToString("00") +
                             DateTime.Now.Day.ToString("00") + "-" +
                             DateTime.Now.Hour.ToString("00") +
                             DateTime.Now.Minute.ToString("00") +
                             DateTime.Now.Second.ToString("00") + ".png";
            ScreenCapture.CaptureScreenshot((Application.dataPath + "/" + imgName), size);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        
        [MenuItem("CONTEXT/Component/Move To Top", priority = 2)]
        private static void MoveToTop(MenuCommand menuCommand)
        {
            Component component = menuCommand.context as Component;
            if (component == null) return;
            // Move the component to the top of the list
            for (int i = component.gameObject.GetComponents<Component>().Length - 1; i >= 0; i--)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(component);
            }
        }

        [MenuItem("Window/Toggle Inspector Lock %q")] // %q is Meaning 'Ctrl + Q'
        private static void ToggleLock()
        {
            ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
    }
}
