using System.Collections.Generic;
using UnityEngine;
public class ConditionDB
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(Mathf.Max(1, pokemon.MaxHP / 8));
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by poison!");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(Mathf.Max(1, pokemon.MaxHP / 16));
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by burn!");
                    Debug.Log($"{pokemon.Base.Name} hurt itself for {Mathf.Max(1, pokemon.MaxHP / 16)} damage due to burn!");
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "fell asleep!",
                OnStart = (Pokemon pokemon) =>
                {
                    // Ngủ từ 1 đến 3 lượt
                    pokemon.StatusTime = Random.Range(1, 4);
                    Debug.Log($"{pokemon.Base.Name} will sleep for {pokemon.StatusTime} turns");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true; // có thể hành động
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is fast asleep...");
                    return false; // không thể hành động
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
        {
            Name = "Paralysis",
            StartMessage = "is paralyzed! It may be unable to move!",

            OnStart = (Pokemon pokemon) =>
            {
                // Giảm Speed 50%
                pokemon.StatModifiers[Stat.Speed] -= 6; // tương đương giảm 2 stage (~50%)
            },

            OnBeforeMove = (Pokemon pokemon) =>
            {
                if (Random.Range(1, 5) == 1) // 25%
                {
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is fully paralyzed!");
                    return false;
                }
                return true;
            }
        }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Frozen",
                StartMessage = "was frozen solid!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    // 20% tỉ lệ tan băng mỗi lượt
                    if (Random.Range(1, 6) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} thawed out!");
                        return true;
                    }

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is frozen solid...");
                    return false;
                }
            }
        },
    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz
}
