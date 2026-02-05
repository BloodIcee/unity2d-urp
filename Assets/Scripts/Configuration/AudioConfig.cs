using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [System.Serializable]
    public struct SfxData
    {
        public SfxType Type;
        public AudioClip Clip;
    }

    [SerializeField] private SfxData[] sfxConfigs;
    [SerializeField] private int poolSize = 10;

    public int PoolSize => poolSize;

    public Dictionary<SfxType, AudioClip> GetClipsDictionary()
    {
        return sfxConfigs.ToDictionary(x => x.Type, x => x.Clip);
    }
}
