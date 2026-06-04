using System.Collections.Generic;
using UnityEngine;

public class VolatileConditionDB
{
    public static Dictionary<VolatileConditionID, Condition> Conditions { get; set; } = new Dictionary<VolatileConditionID, Condition>
    {
        {
            VolatileConditionID.conf,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "became confused!",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"{pokemon.Base.Name} is confused for {pokemon.VolatileStatusTime} turns");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} snapped out of confusion!");
                        return true;
                    }

                    pokemon.VolatileStatusTime--;

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused!");

                    if (Random.value < 0.33f)
                    {
                        int damage = Mathf.Max(1, pokemon.MaxHP / 8);
                        pokemon.UpdateHP(damage);
                        Debug.Log($"{pokemon.Base.Name} hurt itself for {damage} damage due to confusion!");
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself in its confusion!");
                        return false;
                    }

                    return true;
                }
            }
        }

        // Bạn có thể thêm flinch, infatuation, curse... tại đây sau
    };
}

public enum VolatileConditionID
{
    none, conf, flinch, infatuation, taunt, encore, disable, curse
}
