using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

public class AudioManager : MonoBehaviour
{
    private AudioConfig config;
    private List<AudioSource> _pool;
    private Dictionary<SfxType, AudioClip> _clips;
    private Transform poolContainer;

    [Inject]
    public void Construct(AudioConfig audioConfig)
    {
        this.config = audioConfig;
        Initialize();
    }

    private void Initialize()
    {
        if (config == null)
        {
            Debug.LogError("No config");
            return;
        }

        _clips = config.GetClipsDictionary();
        _pool = new List<AudioSource>();
        
        GameObject containerGo = new GameObject("AudioPool");
        containerGo.transform.SetParent(transform);
        poolContainer = containerGo.transform;

        for (int i = 0; i < config.PoolSize; i++)
        {
            CreateSource();
        }
    }

    private AudioSource CreateSource()
    {
        var go = new GameObject($"AudioSource_{_pool.Count}");
        go.transform.SetParent(poolContainer);
        
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        
        _pool.Add(source);
        return source;
    }

    public void PlaySfx(SfxType type)
    {
        if (_clips == null || !_clips.TryGetValue(type, out var clip)) return;
        if (clip == null) return;
        
        var source = _pool.FirstOrDefault(s => !s.isPlaying);
        
        if (source == null)
        {
            source = _pool[0]; 
        }

        source.clip = clip;
        source.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        source.Play();
    }

    public async UniTask PlayGameOverWithDelayAsync()
    {
        PlaySfx(SfxType.GameOver);
        if (_clips.TryGetValue(SfxType.GameOver, out var clip) && clip != null)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length));
        }
    }
}
