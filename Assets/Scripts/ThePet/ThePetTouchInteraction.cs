using DesktopPet.Save;
using DesktopPet.UI;
using UnityEngine;

[RequireComponent(typeof(ThePet))]
[RequireComponent(typeof(ThePetInputManager))]
[RequireComponent(typeof(ThePetStatsManager))]
public class ThePetTouchInteraction : MonoBehaviour
{
    private const float IntimacyIncreaseAmount = 5f;
    private const float TouchCooldownSeconds = 20f;

    private ThePet pet;
    private ThePetInputManager inputManager;
    private ThePetStatsManager statsManager;
    private PetPromptUI promptUI;
    private float nextAvailableTouchTime;

    private void Awake()
    {
        pet = GetComponent<ThePet>();
        inputManager = GetComponent<ThePetInputManager>();
        statsManager = GetComponent<ThePetStatsManager>();
    }

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.PetClicked += HandlePetClicked;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.PetClicked -= HandlePetClicked;
        }
    }

    private void HandlePetClicked()
    {
        if (pet == null || pet.states == null)
        {
            return;
        }

        if (Time.time < nextAvailableTouchTime)
        {
            ShowCooldownPrompt();
            return;
        }

        if (statsManager != null && statsManager.current_stats != null)
        {
            statsManager.current_stats.intimacy += IntimacyIncreaseAmount;
            statsManager.SaveCurrentStats();
        }

        nextAvailableTouchTime = Time.time + TouchCooldownSeconds;
        inputManager?.NotifyInteraction();
        pet.states.Change<StretchState>();
    }

    private void ShowCooldownPrompt()
    {
        if (promptUI == null)
        {
            PetStatsDisplayUI statsDisplayUI = FindFirstObjectByType<PetStatsDisplayUI>();
            if (statsDisplayUI != null)
            {
                promptUI = statsDisplayUI.GetComponent<PetPromptUI>();
                if (promptUI == null)
                {
                    promptUI = statsDisplayUI.gameObject.AddComponent<PetPromptUI>();
                }
            }
        }

        float remainingSeconds = Mathf.Max(0f, nextAvailableTouchTime - Time.time);
        int waitSeconds = Mathf.CeilToInt(remainingSeconds);
        string message = $"请稍等{waitSeconds}秒后再试";

        if (promptUI != null)
        {
            promptUI.ShowPrompt(message, 1.2f);
            return;
        }

        Debug.Log(message);
    }
}
