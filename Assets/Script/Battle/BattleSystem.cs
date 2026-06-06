using System.Collections;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BagScreen, BattleOver,ThrowingBall, UsingItem }

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] MegaScreen megaScreen;
    [SerializeField] GameObject battleCanvas;
    public BattleState state;
    public event Action<bool> OnBattleOver;
    int currentAction;
    int currentMove;
    int currentMember;
    int escapeAttempts = 1;    PokemonParty playerParty;
    Pokemon wildPokemon;
    PokemonParty trainerParty; // store trainer's party for trainer battles
    bool isTrainerBattle = false;  // ← Flag để phân biệt wild vs trainer
    TrainerAI trainerAI;             // ← Instance của AI cho trainer
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, TrainerPersonality? trainerPersonality = null)
    {
        // Nếu truyền TrainerPersonality thì đó là trainer battle
        if (trainerPersonality.HasValue)
        {
            isTrainerBattle = true;
            trainerAI = new TrainerAI(trainerPersonality.Value);
        }
        else
        {
            isTrainerBattle = false;
            trainerAI = null;
        }

        megaScreen.gameObject.SetActive(false);
        escapeAttempts = 1;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    // Convenience method for starting a trainer battle with the full trainer party
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, Pokemon firstTrainerPokemon, TrainerPersonality trainerPersonality)
    {
        isTrainerBattle = true;
        this.trainerParty = trainerParty;
        trainerAI = new TrainerAI(trainerPersonality);

        megaScreen.gameObject.SetActive(false);
        escapeAttempts = 1;
        this.playerParty = playerParty;
        this.wildPokemon = firstTrainerPokemon; // reuse wildPokemon field for current enemy
        StartCoroutine(SetupBattle());
    }
    private IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        if (isTrainerBattle && trainerParty != null)
            yield return dialogBox.TypeDialog($"Trainer sent out {enemyUnit.Pokemon.Base.Name}!");
        else
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");
        ChooseFristTurn();
    }
    void ChooseFristTurn()
    {
        if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
        {
            ActionSelection();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
        playerUnit.Pokemon.RevertMegaForm();
    }

    public void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action:");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }
    public void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    public void OpenBagScreen()
    {
        dialogBox.EnableActionSelector(false);
        state = BattleState.BagScreen;
        inventoryUI.gameObject.SetActive(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;
        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);
        //if the battle is was not changed by RunMove then go to next step
        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        Move move;
        if (isTrainerBattle && trainerAI != null)
        {
            // Trainer battle: dùng AI để chọn move
            move = trainerAI.SelectMove(enemyUnit.Pokemon, playerUnit.Pokemon);
        }
        else
        {
            // Wild pokemon: random move
            move = enemyUnit.Pokemon.GetRandomMove();
        }

        yield return RunMove(enemyUnit, playerUnit, move);
        //if the battle is was not changed by RunMove then go to next step
        if (state == BattleState.PerformMove)
        {
            ActionSelection();
        }
    }
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        //  Kiểm tra trạng thái trước khi thực hiện chiêu
        bool canRunMove = true;

        //  Kiểm tra status chính (poison, paralysis, v.v)
        if (sourceUnit.Pokemon.Status?.OnBeforeMove != null)
        {
            canRunMove &= sourceUnit.Pokemon.Status.OnBeforeMove(sourceUnit.Pokemon);
        }

        //  Kiểm tra volatile status (confusion, infatuation, v.v)
        if (sourceUnit.Pokemon.VolatileStatus?.OnBeforeMove != null)
        {
            canRunMove &= sourceUnit.Pokemon.VolatileStatus.OnBeforeMove(sourceUnit.Pokemon);
        }

        //  Hiển thị các thay đổi trạng thái nếu có
        yield return showStatusChanges(sourceUnit.Pokemon);

        //  Nếu không thể hành động, bỏ lượt
        if (!canRunMove)
        {
            yield return sourceUnit.Hud.UpdateUI();
            yield return new WaitForSeconds(1f);
            yield break;
        }

        //  Trừ PP và thông báo dùng chiêu
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(0.5f);
        targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveBase.MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Pokemon, targetUnit.Pokemon);
        }
        else
        {
            var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
            yield return targetUnit.Hud.UpdateUI();
            yield return ShowDamageDetails(damageDetails);
            yield return RunMoveEffects(move, sourceUnit.Pokemon, targetUnit.Pokemon);
        }

        // Nếu đối phương chết
        if (targetUnit.Pokemon.CurrentHp <= 0)
        {
            targetUnit.PlayFaintAnimation();
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} fainted!");
            yield return new WaitForSeconds(1f);            if (sourceUnit.IsPlayerUnit)
            {
                int exp = CalculateExpGain(sourceUnit.Pokemon, targetUnit.Pokemon, isTrainerBattle);
                yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} gained {exp} EXP!");

                bool leveledUp = sourceUnit.Pokemon.GainExp(exp);
                yield return sourceUnit.Hud.UpdateUI();

                if (leveledUp)
                {
                    sourceUnit.Hud.SetLevel();
                }
            }

            yield return new WaitForSeconds(1f);
            CheckForBattleOver(targetUnit);
        }

        //  Gây damage từ hiệu ứng trạng thái (burn, poison, v.v)
        sourceUnit.Pokemon.OnAfterTurn();
        yield return showStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateUI();

        //  Kiểm tra nếu source chết sau khi dính hiệu ứng
        if (sourceUnit.Pokemon.CurrentHp <= 0)
        {
            sourceUnit.PlayFaintAnimation();
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} fainted!");
            yield return new WaitForSeconds(2f);
            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
    {
        var effects = move.Base.Effects;

        // Tăng/giảm chỉ số
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveBase.MoveTartget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        // Gây trạng thái chính (poison, burn, sleep...)
        if (effects.Status != ConditionID.none)
        {
            int roll = UnityEngine.Random.Range(1, 101);
            if (roll <= effects.StatusChance)
            {
                target.setStatus(effects.Status);
                yield return showStatusChanges(target);
            }
        }

        // Gây volatile status (confusion, flinch, infatuation...)
        if (effects.VolatileStatus != VolatileConditionID.none)
        {
            int roll = UnityEngine.Random.Range(1, 101);
            if (roll <= effects.VolatileStatusChance)
            {
                target.SetVolatileStatus(effects.VolatileStatus);
                yield return showStatusChanges(target);
            }
        }

        yield return showStatusChanges(source);
        yield return showStatusChanges(target);
    }
    int CalculateExpGain(Pokemon attacker, Pokemon defeated, bool isTrainerBattle)
    {
        float a = defeated.Base.ExpYield;
        float l = defeated.Level;
        float t = isTrainerBattle ? 1.5f : 1f;

        float exp = ((a * l * t) / 5f) * Mathf.Pow((2f * l + 10f) / (l + attacker.Level + 10f), 2.5f) + 1;
        return Mathf.FloorToInt(exp);
    }

    IEnumerator showStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            // Nếu đây là trận trainer battle, kiểm tra xem trainer còn Pokemon nào không
            if (isTrainerBattle && trainerParty != null)
            {
                // Dùng AI để chọn Pokemon tiếp theo tốt nhất dựa trên Pokemon hiện tại của player và tính cách của trainer
                var next = trainerAI.ChooseBestPokemon(playerUnit.Pokemon, trainerParty);
                if (next != null)
                {
                    StartCoroutine(TrainerSendOutNextPokemon(next));
                    return;
                }
            }

            BattleOver(true);
        }
    }

    // Trainer gửi Pokemon tiếp theo ra nếu có
    IEnumerator TrainerSendOutNextPokemon(Pokemon next)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Trainer sent out {next.Base.Name}!");
        yield return new WaitForSeconds(0.5f);

        enemyUnit.Setup(next);
        yield return dialogBox.TypeDialog($"{next.Base.Name} appeared!");
        yield return new WaitForSeconds(0.5f);

        ActionSelection();
    }
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog($"A critical hit!");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog($"It's super effective! ");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog($"It's not very effective... ");
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.BagScreen)
        {
            HandleBagSelection();
        }
    }
    void HandleActionSelection()
    {
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            megaScreen.Setup(playerUnit.Pokemon);
            StartCoroutine(MegaEvolvePokemon(playerUnit.Pokemon));
        }
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            ++currentAction;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            --currentAction;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            currentAction += 2;
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            currentAction -= 2;
        }
        currentAction = Mathf.Clamp(currentAction, 0, 3);
        dialogBox.UpdateActionSelection(currentAction);
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Bag
                OpenBagScreen();
            }
            else if (currentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
                StartCoroutine(TryToRun());
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            ++currentMove;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            --currentMove;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            currentMove += 2;
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            currentMove -= 2;
        }
        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }
    void HandlePartySelection()
    {
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            ++currentMember;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            --currentMember;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            currentMember += 2;
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            currentMember -= 2;
        }
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);
        partyScreen.UpdateMemberSelection(currentMember);
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.CurrentHp <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted Pokemon!");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch to the same Pokemon!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    void HandleBagSelection()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            inventoryUI.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonfainted = true;
        if (playerUnit.Pokemon.CurrentHp > 0)
        {
            currentPokemonfainted = false;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}, come back!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Go! {newPokemon.Base.Name}!");
        if (currentPokemonfainted)
        {
            ChooseFristTurn();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }

    }
    public void UseItemOnPokemon(ItemBase item)
    {
        dialogBox.EnableActionSelector(false);
        state = BattleState.UsingItem;
        StartCoroutine(UseItemCoroutine(item));
    }
    private IEnumerator UseItemCoroutine(ItemBase item) // test 
    {
        if (item is PokeballItem pokeball)
        {
            if (wildPokemon == null)
            {
                yield return dialogBox.TypeDialog("There is no wild Pokemon to catch!");
                state = BattleState.Busy;
                yield break;
            }
            yield return TryToCatchPokemon(pokeball);
        }
        else
        {
            yield return dialogBox.TypeDialog($"You used {item.ItemName} on {playerUnit.Pokemon.Base.Name}!");
            item.Use(playerUnit.Pokemon);
            yield return playerUnit.Hud.UpdateUI();
            yield return new WaitForSeconds(1f);
            state = BattleState.Busy;
            StartCoroutine(EnemyMove());
        }
    }

    public IEnumerator TryToCatchPokemon(PokeballItem pokeball)
    {
        state = BattleState.ThrowingBall;

        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} threw a {pokeball.ItemName}!");

        Vector3 throwFromScreen = new Vector3(-1f, 0.5f, 5f); // 5f là z-distance (khoảng cách camera)
        Vector3 worldThrowPos = Camera.main.ViewportToWorldPoint(new Vector3(throwFromScreen.x, throwFromScreen.y, 5f));

        GameObject ballGO = Instantiate(pokeball.PokeballPrefab, worldThrowPos, Quaternion.identity, battleCanvas.transform);
        PokeballAnimationHandler ballAnim = ballGO.GetComponent<PokeballAnimationHandler>();

        // 2. Tính vị trí target là enemy rồi convert sang UI world pos
        Vector3 targetWorldPos = enemyUnit.transform.position + new Vector3(0, 1f, 0);
        Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(targetWorldPos);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            battleCanvas.GetComponent<RectTransform>(),
            targetScreenPos,
            Camera.main,
            out Vector3 targetUIPos
        );

        // Di chuyển bóng tới đối thủ
        float moveDuration = 0.5f;
        float t = 0f;
        Vector3 startPos = ballGO.transform.position;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float progress = t / moveDuration;
            ballGO.transform.position = Vector3.Lerp(startPos, targetUIPos, progress);
            yield return null;
        }

        ballAnim.PlayTouch(); //Gọi hiệu ứng chạm
        yield return new WaitForSeconds(0.2f);

        // Thu nhỏ enemy mượt mà
        Vector3 originalScale = enemyUnit.transform.localScale;
        t = 0f;
        float shrinkDuration = 0.3f;

        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float progress = t / shrinkDuration;
            enemyUnit.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // 3. Tính toán bắt
        bool caught = pokeball.Use(wildPokemon);

        if (caught)
        {
            yield return StartCoroutine(ballAnim.PlayShake(3));

            ballAnim.PlaySuccess();
            yield return dialogBox.TypeDialog($"Gotcha! {wildPokemon.Base.Name} was caught!");
            yield return new WaitForSeconds(1f);
            int exp = CalculateExpGain(playerUnit.Pokemon, wildPokemon, isTrainerBattle: false);
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {exp} EXP!");
            bool leveledUp = playerUnit.Pokemon.GainExp(exp);
            yield return playerUnit.Hud.UpdateUI();
            Pokemon newPoke = new Pokemon(wildPokemon.Base, wildPokemon.Level, wildPokemon.CurrentHp);
            playerParty.Pokemons.Add(newPoke);
            Destroy(ballGO);
            BattleOver(true);
        }
        else
        {
            int shakeCount = UnityEngine.Random.Range(1, 3);
            yield return StartCoroutine(ballAnim.PlayShake(shakeCount));

            ballAnim.PlayBreakOut();

            // Phóng to lại enemy
            t = 0f;
            float growDuration = 0.3f;

            while (t < growDuration)
            {
                t += Time.deltaTime;
                float progress = t / growDuration;
                enemyUnit.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
                yield return null;
            }

            yield return dialogBox.TypeDialog($"{wildPokemon.Base.Name} broke free!");
            yield return new WaitForSeconds(1f);

            Destroy(ballGO);
            StartCoroutine(EnemyMove());
        }
    }


    IEnumerator TryToRun()
    {
        int playerSpeed = playerUnit.Pokemon.Base.Speed;
        int enemySpeed = enemyUnit.Pokemon.Base.Speed;

        // Tính cơ hội chạy trốn đúng theo công thức Pokémon Gen I–V
        int chance = (playerSpeed * 128 / enemySpeed) + 30 * escapeAttempts;
        chance = Mathf.Clamp(chance, 0, 255); // Đảm bảo không vượt quá

        int roll = UnityEngine.Random.Range(0, 256); // [0..255]

        dialogBox.EnableActionSelector(false);

        if (roll < chance)
        {
            yield return dialogBox.TypeDialog("You ran away successfully!");
            BattleOver(true);
        }
        else
        {
            yield return dialogBox.TypeDialog("You couldn't get away!");
            escapeAttempts++; // Lần sau dễ hơn
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator MegaEvolvePokemon(Pokemon pokemon)
    { 
        if (pokemon.TryMegaEvolve(pokemon.HeldItem.ItemName)) // Giả sử bạn có `HeldItem`
        {
            yield return dialogBox.TypeDialog($"{pokemon.Base.Name} has Mega Evolved!");
            playerUnit.Hud.SetData(pokemon); // cập nhật lại hình ảnh/HP nếu cần
            playerUnit.Setup(pokemon); // cập nhật BattleUnit với Pokémon mới
            yield return new WaitForSeconds(10f);

        }
        else
        {
            yield return dialogBox.TypeDialog($"Mega Evolution failed!");
        }
    }

}
