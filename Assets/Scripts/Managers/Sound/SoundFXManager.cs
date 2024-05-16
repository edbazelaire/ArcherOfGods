using System.Collections;
using Tools;
using UnityEngine;

namespace Assets.Scripts.Managers.Sound
{
    public class SoundFXManager : MonoBehaviour
    {
        public static SoundFXManager Instance;

        private AudioSource m_AudioSource;

        private void Awake()
        {
            Instance = this;
            m_AudioSource = Finder.FindComponent<AudioSource>(gameObject);
        }

        public AudioSource PlaySoundFXClip(AudioClip audioClip, Transform transform, float volume = 1)
        {
            // spawn audio source
            AudioSource audioSource = Instantiate(m_AudioSource, transform);
            audioSource.loop = true;

            // assign clip
            audioSource.clip = audioClip;

            // assign volume
            audioSource.volume = volume;

            // play sound
            audioSource.Play();

            // return audio source
            return audioSource;
        }

        public void PlayOnce(AudioClip audioClip, float volume = 1)
        {
            var audioSource = PlaySoundFXClip(audioClip, null, volume);
            audioSource.loop = false;

            Destroy(audioSource.gameObject, audioClip.length);
        }
    }
}