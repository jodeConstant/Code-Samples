using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PikkuliHoiva
{
    [System.Serializable]
    public class AudioClipFeedElement
    {
        public AudioClip clip;
        public float volumeIncreaseMultiplier = 0f;
        public float delay = 0f;
        public float endPadding = 1f;
        public float EventWaitTime { get { return ((clip != null) ? clip.length : 0f) + endPadding; } }
        public UnityEvent OnWaitComplete;
    }

    public class AudioClipFeedScript : MonoBehaviour
    {
        public AudioSource audioSource;
        public List<AudioClipFeedElement> audioClipFeedList;
        //public UnityEvent OnFeedListComplete;
        
        AudioClip currentClip = null;
        float clipWaitTime = 0f;
        float currentVolumeMultiplier;

        void Start()
        {
            StartCoroutine(AudioClipFeedRoutine());
        }
        
        IEnumerator AudioClipFeedRoutine()
        {
            int i = 0, c = audioClipFeedList.Count;
            for ( ; i < c; i++)
            {
                yield return new WaitForSeconds(audioClipFeedList[i].delay);
                currentClip = audioClipFeedList[i].clip;
                clipWaitTime = audioClipFeedList[i].EventWaitTime;
                currentVolumeMultiplier = audioClipFeedList[i].volumeIncreaseMultiplier + 1f;
                if (currentClip != null)
                {
                    audioSource.PlayOneShot(currentClip, currentVolumeMultiplier);
                }
                yield return new WaitForSeconds((clipWaitTime > 0f) ? clipWaitTime : 0.1f);
                audioClipFeedList[i].OnWaitComplete.Invoke();
            }
            //OnFeedListComplete.Invoke();
        }

        public void LoopCurrentClip(float repeats)
        {
            if ((currentClip != null) && (repeats > 0))
            {
                StartCoroutine(AudioClipLoopRoutine(currentClip, repeats, (clipWaitTime > 0f) ? clipWaitTime : 0.1f, currentVolumeMultiplier));
            }
        }

        IEnumerator AudioClipLoopRoutine(AudioClip clip, float repeats, float waitTime, float volumeMultiplier = 1f)
        {
            while (repeats > 0)
            {
                audioSource.PlayOneShot(clip, volumeMultiplier);
                repeats--;
                yield return new WaitForSeconds(waitTime);
            }
        }
    }
}
