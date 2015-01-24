using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.ComponentModel;

public enum HelpText
{
    [Description("")]
    NoText,
    [Description("Hold Action to drag the corpse")]
    ActionToDragCorpse,
    [Description("Release Action to drop the corpse")]
    ActionToDropCorpse,
    [Description("Press Action to read")]
    ActionToRead,
    [Description("Press Action to close")]
    ActionToClose,
    [Description("This is a one way door!")]
    WrongDirection
}

public class MainCanvas : MonoBehaviour
{
    private static MainCanvas instance;

    private Text helpText;
    private HelpText currentHelpText;
    private RectTransform upperLetterBox;
    private RectTransform lowerLetterBox;
    private Image letterBoxFlash;

    private const float TargetAlpha = 0.5f;

    public static MainCanvas Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        instance = this;

        helpText = transform.FindChild("HelpText").GetComponent<Text>();
        helpText.CrossFadeAlpha(0, 0, true);

        upperLetterBox = transform.FindChild("UpperLetterBox").GetComponent<RectTransform>();
        lowerLetterBox = transform.FindChild("LowerLetterBox").GetComponent<RectTransform>();
        letterBoxFlash = transform.FindChild("LetterBoxFlash").GetComponent<Image>();

        upperLetterBox.anchorMax = new Vector2(1, 0.5f); // 1, 0
        lowerLetterBox.anchorMin = new Vector2(0, 0.5f); // 0, 1
        letterBoxFlash.CrossFadeAlpha(TargetAlpha, 0, true);

        FadeIn();
    }

    public void HideHelpText(HelpText text)
    {
        if (currentHelpText != text)
            return;
        currentHelpText = HelpText.NoText;
        helpText.CrossFadeAlpha(0, 0.33f, true);
    }

    public void ShowHelpText(HelpText text, float duration = -1)
    {
        if (text == currentHelpText)
        {
            return;
        }

        currentHelpText = text;
        helpText.text = text.GetDescription();
        helpText.CrossFadeAlpha(1, 0.33f, true);

        if (duration > 0)
        {
            StartCoroutine(FadeOutHelpText(duration));
        }
    }

    private IEnumerator FadeOutHelpText(float duration)
    {
        yield return new WaitForSeconds(duration);
        HideHelpText(currentHelpText);
    }

    public void FadeOut(float duration = 1)
    {
        StartCoroutine(FadeOutDelay(duration));
        letterBoxFlash.CrossFadeAlpha(TargetAlpha, duration, false);
    }

    private IEnumerator FadeOutDelay(float duration)
    {
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;

            upperLetterBox.anchorMax = new Vector2(1, 0.5f / duration * time); // 1, 0
            lowerLetterBox.anchorMin = new Vector2(0, 1 - 0.5f / duration * time); // 0, 1

            yield return null;  
        }
    }

    public void FadeIn(float duration = 1)
    {
        letterBoxFlash.CrossFadeAlpha(0, duration, false);
        StartCoroutine(FadeInAnimation(duration));
    }

    private IEnumerator FadeInAnimation(float duration)
    {
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;

            upperLetterBox.anchorMax = new Vector2(1, 0.5f - 0.5f / duration * time); // 1, 0
            lowerLetterBox.anchorMin = new Vector2(0, 0.5f + 0.5f / duration * time); // 0, 1

            yield return null;  
        }
    }
}
