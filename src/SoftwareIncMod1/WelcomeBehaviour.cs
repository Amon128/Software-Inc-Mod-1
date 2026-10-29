using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoftwareIncMod1
{
    // Sample behaviour: shows a configurable message when a game loads and counts
    // how many times each save has been loaded. Replace it with your own logic.
    //
    // ModBehaviours are Unity MonoBehaviours. The game creates one instance of each
    // when the mod loads and keeps it enabled only while the mod is active.
    public class WelcomeBehaviour : ModBehaviour
    {
        public static WelcomeBehaviour Instance;

        private const string MessageSetting = "WelcomeMessage";
        private const string DefaultMessage = "SoftwareIncMod1 is active.";

        private string _message;
        private int _timesLoaded;

        public string Message
        {
            get { return _message; }
        }

        void Awake()
        {
            Instance = this;
            // Global settings are shared by every save and stored next to the mod.
            _message = LoadSetting(MessageSetting, DefaultMessage);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetMessage(string message)
        {
            _message = message;
            SaveSetting(MessageSetting, message);
        }

        // Mods start deactivated. This runs at launch if the player has the mod
        // enabled, and again every time they toggle it on.
        public override void OnActivate()
        {
            Debug.Log("[SoftwareIncMod1] Activated");
        }

        // Undo everything the mod changed. The behaviour is disabled right after this.
        public override void OnDeactivate()
        {
            Debug.Log("[SoftwareIncMod1] Deactivated");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Scenes include MainMenu, MainScene (an active game) and Customization.
            if (!isActiveAndEnabled || scene.name != "MainScene" || string.IsNullOrEmpty(_message))
            {
                return;
            }
            WindowManager.Instance.ShowMessageBox(_message, true, DialogWindow.DialogType.Information);
        }

        // Per-save data, stored in the save file. Values must be value types or
        // classes with an empty constructor. mode tells a full save apart from a
        // build mode or company save; avoid saving anything for build mode saves.
        public override void Serialize(WriteDictionary data, GameReader.LoadMode mode)
        {
            data["TimesLoaded"] = _timesLoaded;
        }

        public override void Deserialize(WriteDictionary data, GameReader.LoadMode mode)
        {
            _timesLoaded = data.Get("TimesLoaded", 0) + 1;
            Debug.Log("[SoftwareIncMod1] This save has been loaded " + _timesLoaded + " time(s)");
        }
    }
}
