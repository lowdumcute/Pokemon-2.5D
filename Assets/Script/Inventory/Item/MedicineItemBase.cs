using UnityEngine;

[CreateAssetMenu(menuName = "Item/Medicine")]
public class MedicineItemBase : ItemBase
{
    [SerializeField] private int healAmount;

    public override bool Use(Pokemon pokemon)
    {
        if (pokemon.CurrentHp < pokemon.MaxHP)
        {
            pokemon.IncreaseHP(healAmount); // Hàm bạn phải có trong class Pokemon
            return true;
        }
        return false;
    }
}
