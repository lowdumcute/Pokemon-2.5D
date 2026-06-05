# 🚀 **TrainerAI Quick Reference**

## **Cách Dùng TrainerAI**

### **1. Setup NPC Trainer với Personality**

**Trong Inspector:**
```csharp
[SerializeField] TrainerPersonality personality = TrainerPersonality.Tactical;
```

**Hoặc qua code:**
```csharp
public class MyNPCTrainer : MonoBehaviour
{
    private TrainerAI trainerAI;
    
    void Start()
    {
        trainerAI = new TrainerAI(TrainerPersonality.Aggressive);
    }
}
```

---

## **2. Các Personality Có Sẵn**

### 🔥 **Aggressive**
- **Tính chất:** Tấn công mạnh, ít phòng chỉ
- **Move Priority:** Damage cao + type advantage
- **Heal:** Hiếm khi heal
- **Switch:** Khi bị ưỡng rất nặng (> 1.5x) hoặc HP < 20%
- **Best For:** Early-game trainer, henchman

**Example:**
```
Pikachu (Aggressive personality)
→ Thù Water type
→ Chọn Thunderbolt (super-effective, high damage)
→ Finish opponent ASAP
```

---

### 🧠 **Tactical**
- **Tính chất:** Cân bằng, chiến lược
- **Move Priority:** Type advantage + damage + PP management
- **Heal:** Heal khi cần thiết
- **Switch:** Khi bị ưỡng (> 1x) hoặc HP < 30%
- **Best For:** Gym leader, strong trainer, rival

**Example:**
```
Alakazam (Tactical personality)
→ Thù Bug type
→ Check moves: Psychic (super-effective), Focus Blast (high power)
→ Xét PP, type eff, opponent HP
→ Chọn optimal move
```

---

### 😰 **Coward**
- **Tính chất:** Phòng chỉ, heal thường xuyên
- **Move Priority:** Heal/buff > damage
- **Heal:** Heal sớm (HP < 40%), buff (HP < 60%)
- **Switch:** Khi HP < 25%
- **Best For:** Support trainer, annoying trainer, rival

**Example:**
```
Blissey (Coward personality)
→ HP 45% → ưu tiên Recover (score 150)
→ HP 28% < 25% → có thể switch Pokemon
→ Kéo dài trận chiến
```

---

## **3. Custom Personality (Future)**

```csharp
// Tạo personality custom (cần thêm enum)
public enum TrainerPersonality
{
    Aggressive,
    Tactical,
    Coward,
    // Custom thêm:
    Balanced,  // Offensive + defensive
    Defensive,  // Full support
    Random,     // Ngẫu nhiên
}
```

---

## **4. Move Selection Logic**

### **Aggressive Trainer chọn move như nào?**
```
1. Tính damage của mỗi move
2. Thêm bonus: typeEff * 3f
3. Nếu opponent HP < 20% → score *= 1.5f (finish priority)
4. Thêm random: ±2f
5. Chọn move có score cao nhất
```

### **Tactical Trainer?**
```
1. Nếu status move → đánh giá (buff/debuff score)
2. Nếu damage move:
   - Tính damage
   - Thêm type advantage: * 2.5f
   - Opponent HP < 30% → * 1.3f
   - Random: ±1f
3. Chọn move có score cao nhất
```

### **Coward Trainer?**
```
1. Nếu HP < 40% + Self Status move → score = 150 (PRIORITY)
2. Nếu HP < 60% + Self Status move → score = 80
3. Nếu opponent HP < 20% → * 1.2f (có cơ hội finish)
4. Chọn move có score cao nhất
```

---

## **5. Helper Methods**

```csharp
// Tính damage potential của move
private float EstimateDamagePotential(Move move, Pokemon attacker, Pokemon defender)
{
    // Tính damage dựa trên power, type effectiveness
    // Return: damage score
}

// Check type advantage
private float GetTypeEffectiveness(Move move, Pokemon defender)
{
    // Return: 0.5f (không hiệu quả), 1f (neutral), 2f (super-effective)
}

// Đánh giá status move (buff/debuff)
private float EvaluateStatusMove(Move move, Pokemon attacker, Pokemon defender)
{
    // Return: score dựa trên effect type và quality
}

// Check xem có nên switch Pokemon không (UNUSED hiện tại)
public bool ShouldSwitchPokemon(Pokemon current, Pokemon player, PokemonParty party)
{
    // Return: true nếu nên switch
}
```

---

## **6. Integration với BattleSystem**

**Hiện tại:**
```csharp
// BattleSystem gọi TrainerAI khi enemy turn
Move move = trainerAI.SelectMove(enemyUnit.Pokemon, playerUnit.Pokemon);
yield return RunMove(enemyUnit, playerUnit, move);
```

**Future (Switch Pokemon):**
```csharp
if (trainerAI.ShouldSwitchPokemon(enemyUnit.Pokemon, playerUnit.Pokemon, trainerParty))
{
    // Switch Pokemon logic
    var nextPokemon = trainerParty.GetHealthyPokemon();
    // Setup next Pokemon...
}
```

**Future (Item Usage):**
```csharp
// Coward trainer dùng item heal
if (trainerAI.personality == TrainerPersonality.Coward && hp < 30%)
{
    UseItem(FullRestore);
}
```

---

## **7. Test Cases**

### **Test 1: Aggressive vs Water**
```
Setup: Pikachu (Aggressive, Moves: Thunderbolt, Thunder, Quick-Attack)
Opponent: Gyarados (Water type, HP = 60%)

Expected:
→ Chọn Thunderbolt (super-effective, score cao nhất)
→ Ko chọn Quick-Attack (damage thấp)
```

### **Test 2: Coward HP < 40%**
```
Setup: Blissey (Coward, Moves: Recover, Dazzling Gleam, Soft-boiled)
HP: 42%

Expected:
→ Ưu tiên Recover hoặc Soft-boiled (score = 150)
→ Ko chọn Dazzling Gleam (damage move)
```

### **Test 3: Tactical vs opponent HP < 30%**
```
Setup: Alakazam (Tactical, HP = 80%, Opponent HP = 20%)
Moves: Psychic, Focus Blast, Calm Mind

Expected:
→ Chọn damage move (Psychic hoặc Focus Blast)
→ Vì opponent HP < 30% → finish priority
```

### **Test 4: Randomness**
```
Setup: Chạy multiple times (10x) lần với cùng situation
Aggressive trainer, 2 moves có score gần nhau

Expected:
→ Không phải lúc nào chọn move giống nhau
→ Có randomness ±2f
```

---

## **8. Debugging**

```csharp
// Add debug log để xem move selection
public Move SelectMove(Pokemon attacker, Pokemon defender)
{
    Debug.Log($"[TrainerAI] {personality} selecting move...");
    Debug.Log($"Attacker: {attacker.Base.Name}, Defender: {defender.Base.Name}");
    Debug.Log($"Defender HP: {defender.CurrentHp}/{defender.MaxHP}");
    
    Move selectedMove = GetMove(attacker, defender);
    Debug.Log($"Selected: {selectedMove.Base.Name}");
    return selectedMove;
}
```

---

## **9. Performance Note**

- ✅ TrainerAI là lightweight (không loop)
- ✅ SelectMove() gọi 1 lần per turn
- ✅ Không có async operations
- ✅ Safe cho real-time battles

---

## **10. Known Limitations**

- ⚠️ **Không switch Pokemon** (ShouldSwitchPokemon ko được gọi)
- ⚠️ **Không dùng Item** (chưa implement)
- ⚠️ **Không predict player** (ko biết player chọn gì)
- ⚠️ **Random có seed** (Random.Range ko controllable)

**Workaround:**
```csharp
// Nếu muốn reproducible random:
private Random rng = new Random(seed);
score += rng.Range(-2f, 2f);
```

---

## **Version History**

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Initial | Basic 3 personalities |
| 1.1 | June 5 | Add random, finish logic, switch stub |
| 1.2 | TBD | Switch Pokemon integration |
| 1.3 | TBD | Item usage |

---

**File:** `Assets/Script/Battle/TrainerAI.cs`  
**Status:** ✅ Ready to use  
**Next:** Integrate SwitchPokemon + ItemUsage vào BattleSystem
