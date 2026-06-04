using UnityEngine;

public enum PokeballType
{
    Pokeball,       // 1.0
    GreatBall,      // 1.5
    UltraBall,      // 2.0
    MasterBall,     // 255 (100%)
    TimerBall,      // Tăng theo thời gian (chưa xử lý)
    DuskBall,       // Ban đêm, hang động (chưa xử lý)
    QuickBall,      // Turn đầu (chưa xử lý)
    RepeatBall,     // Nếu đã bắt trước đó (chưa xử lý)
    NetBall,        // Cho hệ Water/Bug (chưa xử lý)
    DiveBall,       // Khi dưới nước (chưa xử lý)
    LuxuryBall,     // Chỉ tăng Friendship, bắt như Poké Ball
    HealBall,       // Hồi máu khi bắt
    FastBall        // Dành cho Pokémon có tốc độ cao
}

[CreateAssetMenu(menuName = "Item/ Pokeball")]
public class PokeballItem : ItemBase
{
    [SerializeField] private PokeballType ballType;

    public PokeballType BallType => ballType;
    [SerializeField] private GameObject pokeballPrefab; // Prefab để ném
    public GameObject PokeballPrefab => pokeballPrefab;

    public float CatchRate
    {
        get
        {
            switch (ballType)
            {
                case PokeballType.Pokeball: return 1f;
                case PokeballType.GreatBall: return 1.5f;
                case PokeballType.UltraBall: return 2f;
                case PokeballType.MasterBall: return 255f; // Always catch
                case PokeballType.LuxuryBall: return 1f;
                case PokeballType.HealBall: return 1f;
                case PokeballType.FastBall: return 1f; // Mặc định, có thể cải tiến thêm

                // Các loại bóng có hiệu ứng đặc biệt
                case PokeballType.TimerBall:
                case PokeballType.DuskBall:
                case PokeballType.QuickBall:
                case PokeballType.RepeatBall:
                case PokeballType.NetBall:
                case PokeballType.DiveBall:
                    return 1f; // Default. Nên mở rộng sau dựa vào điều kiện
                default: return 1f;
            }
        }
    }

    public override bool Use(Pokemon target)
    {
        float catchRate = CatchRate;

        // Master Ball: bắt luôn
        if (ballType == PokeballType.MasterBall)
            return true;

        // Công thức Gen 3+
        float a = (3f * target.CurrentHp - 2f * target.MaxHP) * target.Base.CatchRate * catchRate / (3f * target.CurrentHp);

        if (a >= 255f)
        {
            return true;
        }

        float b = 1048560f / Mathf.Sqrt(Mathf.Sqrt(16711680f / a));
        for (int i = 0; i < 4; i++)
        {
            if (Random.Range(0, 65536) >= b)
                return false;
        }

        return true;
    }
}
