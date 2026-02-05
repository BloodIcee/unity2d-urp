using System;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SaveLoadService : ISaveLoadService
{
    private const string SAVE_FILE_NAME = "gamestate.json";
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    public async UniTask SaveGameStateAsync(GameSaveData data)
    {
        try
        {
            data.SaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string json = JsonUtility.ToJson(data, false);
            
            string path = SaveFilePath;
            
            await UniTask.SwitchToThreadPool();
            await File.WriteAllTextAsync(path, json);
            await UniTask.SwitchToMainThread();
        }
        catch (Exception)
        {
            await UniTask.SwitchToMainThread();
        }
    }

    public async UniTask<GameSaveData> LoadGameStateAsync()
    {
        try
        {
            string path = SaveFilePath;
            if (!File.Exists(path))
                return new GameSaveData();

            await UniTask.SwitchToThreadPool();
            string json = await File.ReadAllTextAsync(path);
            await UniTask.SwitchToMainThread();

            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            return data ?? new GameSaveData();
        }
        catch (Exception)
        {
            await UniTask.SwitchToMainThread();
            return new GameSaveData();
        }
    }

    public bool HasSaveData()
    {
        return File.Exists(SaveFilePath);
    }

    public void ClearSaveData()
    {
        try
        {
            if (File.Exists(SaveFilePath))
                File.Delete(SaveFilePath);
        }
        catch (Exception)
        {
        }
    }
}
