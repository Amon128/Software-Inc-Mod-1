using UnityEngine;

namespace SoftwareIncMod1
{
    // Every code mod has exactly one ModMeta implementation. It names the mod,
    // builds its panel under Options > Mods and can take over saving if needed.
    public class ModMain : ModMeta
    {
        // The game keys this mod's save data by Name, so renaming it after
        // release orphans the data in existing saves.
        public override string Name
        {
            get { return "SoftwareIncMod1"; }
        }

        // Builds this mod's panel in the options window. inGame is true when the
        // options were opened from a running game rather than the main menu.
        public override void ConstructOptionsScreen(RectTransform parent, bool inGame)
        {
            var label = WindowManager.SpawnLabel();
            label.text = "Message shown when a game loads (leave empty to disable):";
            WindowManager.AddElementToElement(label.gameObject, parent.gameObject,
                new Rect(0, 0, 400, 32), new Rect(0, 0, 0, 0));

            var input = WindowManager.SpawnInputbox();
            input.text = WelcomeBehaviour.Instance != null ? WelcomeBehaviour.Instance.Message : "";
            input.onValueChanged.AddListener(value =>
            {
                if (WelcomeBehaviour.Instance != null)
                {
                    WelcomeBehaviour.Instance.SetMessage(value);
                }
            });
            WindowManager.AddElementToElement(input.gameObject, parent.gameObject,
                new Rect(0, 36, 400, 32), new Rect(0, 0, 0, 0));
        }
    }
}
