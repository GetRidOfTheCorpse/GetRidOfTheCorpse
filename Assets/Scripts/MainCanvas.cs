using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.ComponentModel;

public enum HelpText {
    [Description("")]
    NoText,
    [Description("Hold Action to drag the corpse")]
    ActionToDragCorpse,
    [Description("Release Action to drop the corpse")]
    ActionToDropCorpse,
    [Description("Press Action to read")]
    ActionToRead,
    [Description("Press Action to close")]
    ActionToClose
}

public class MainCanvas : MonoBehaviour
{
    private static MainCanvas instance;

    private Text helpText;
    private HelpText currentHelpText;

    public static MainCanvas Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        instance = this;

        helpText = transform.FindChild("HelpText").GetComponent<Text>();
        helpText.CrossFadeAlpha(0, 0, true);
    }

    public void HideHelpText(HelpText text) {
        if (currentHelpText != text)
            return;
        currentHelpText = HelpText.NoText;
        helpText.CrossFadeAlpha(0, 0.33f, true);
    }

    public void ShowHelpText(HelpText text, float duration = -1) {
        if(text == currentHelpText) {
            return;
        }

        currentHelpText = text;
        helpText.text = text.GetDescription();
        helpText.CrossFadeAlpha(1, 0.33f, true);

        if(duration > 0) {
            StartCoroutine(FadeOutHelpText(duration));
        }
    }

    private IEnumerator FadeOutHelpText(float duration) {
        yield return new WaitForSeconds(duration);
        HideHelpText(currentHelpText);
    }
}
