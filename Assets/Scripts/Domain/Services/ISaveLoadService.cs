using Cysharp.Threading.Tasks;

public interface ISaveLoadService
{
    UniTask SaveGameStateAsync(GameSaveData data);
    UniTask<GameSaveData> LoadGameStateAsync();
    bool HasSaveData();
    void ClearSaveData();
}
