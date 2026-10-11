using UnityEngine;
namespace RollingPanda
{
    public sealed class MovementAudioArt : ScriptableObject
    {
        public AudioClip[] Steps;
        public AudioClip Jump;
        public AudioClip Slide;
        public AudioClip Percussion;
        [Range(0,1)] public float PercussionVolume=.15f;
    }
}
