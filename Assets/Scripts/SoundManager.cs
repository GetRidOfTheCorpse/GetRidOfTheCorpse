using UnityEngine;
using System.Collections;
using System.Linq;

public enum SoundEffect {
    Hey,
    PickUpLine,
    DropLine,
    Collect,
    Decollect
}

[System.Serializable]
public struct SoundEffectEnum {
    public SoundEffect soundEffect;
    public AudioClip audioClip;
}

public class SoundManager : MonoBehaviour
{
    
    private static SoundManager instance;

    public static SoundManager Instance
    {
        get { return instance; }
    }

    public SoundEffectEnum[] soundEffects;

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    // Use this for initialization
    void Start()
    {
        var vol = audio.volume;
        audio.volume = 0;
        audio.Play();
        audio.loop = true;

        StartCoroutine(FadeSound(5, vol));
    }

    IEnumerator FadeSound(float duration, float volume) {
        float time = 0;
        while(time < duration) {
            time += Time.deltaTime;
            audio.volume = Mathf.Lerp(audio.volume, volume, Time.deltaTime);
            yield return null;
        }

        audio.volume = volume;
    }

    public void OneShot(SoundEffect clip, GameObject go) {
        var audioClip = soundEffects.Where((e) => e.soundEffect == clip);

        var enumerable = audioClip.ToArray();
        if(enumerable.Length == 0) {
            return;
        }

        var source = go.GetComponent<AudioSource>();
        if(source == null) {
            source = go.AddComponent<AudioSource>();
        }
        
        source.PlayOneShot(enumerable[0].audioClip);
        source.minDistance = 100000;
        source.maxDistance = 100000;
    }
}
