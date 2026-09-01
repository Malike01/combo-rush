using UnityEngine;

public abstract class JokerBase : MonoBehaviour
{
    [Header("Joker Ayarları")]
    public string jokerName;
    
    protected virtual void Start()
    {
        InitializeJoker();
    }

    protected virtual void OnDestroy()
    {
        CleanupJoker();
    }

    protected abstract void InitializeJoker();
    
    protected abstract void CleanupJoker();

    public abstract void ExecuteJokerEffect();
}