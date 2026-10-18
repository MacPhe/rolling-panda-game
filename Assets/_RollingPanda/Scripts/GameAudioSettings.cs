using UnityEngine;
namespace RollingPanda
{
    public static class GameAudioSettings
    {
        static readonly string[] Keys={"RollingPanda.Volume","RollingPanda.MusicVolume","RollingPanda.EffectsVolume","RollingPanda.AmbienceVolume"};
        static readonly float[] levels=new float[4];
        static bool loaded;
        public static float Music=>Get(1);
        public static float Effects=>Get(2);
        public static float Ambience=>Get(3);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset(){loaded=false;}
        public static float Get(int channel)
        {
            if(!loaded){for(int i=0;i<4;i++)levels[i]=Mathf.Clamp01(PlayerPrefs.GetFloat(Keys[i],1));loaded=true;AudioListener.volume=levels[0];}
            return levels[Mathf.Clamp(channel,0,3)];
        }
        public static void Set(int channel,float value)
        {
            Get(0);channel=Mathf.Clamp(channel,0,3);levels[channel]=float.IsNaN(value) ? 0 : Mathf.Clamp01(value);
            if(channel==0)AudioListener.volume=levels[0];
        }
        public static void Save(){Get(0);for(int i=0;i<4;i++)PlayerPrefs.SetFloat(Keys[i],levels[i]);PlayerPrefs.Save();}
    }
}
