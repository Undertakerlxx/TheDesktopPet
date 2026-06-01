using System;
using System.Collections;
using System.IO;
using DesktopPet.Accounts;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PetSkinManager : MonoBehaviour
{
    private const string DefaultLibraryResourcePath = "PetSkinLibrary";
    private const string IdleStateName = "Idle";
    private const string HappyStateName = "Happy";
    private const string HungryStateName = "Hungry";
    private const string LyingStateName = "Lying";
    private const string SadStateName = "Sad";
    private const string SleepStateName = "Sleep";
    private const string StretchStateName = "Stretch";
    private const string BaseIdleClipName = "CatIdle";
    private const string BaseHappyClipName = "CatHappy";
    private const string BaseHungryClipName = "CatHungry";
    private const string BaseLyingClipName = "CatLying";
    private const string BaseSadClipName = "CatSad";
    private const string BaseSleepClipName = "CatSleep";
    private const string BaseStretchClipName = "CatStretch";

    public PetSkinLibrary skinLibrary;
    public Animator animatorComponent;

    [SerializeField] private int selectedSkinIndex;

    private RuntimeAnimatorController baseController;
    private AnimatorOverrideController overrideController;
    private AnimationClip baseIdleClip;
    private AnimationClip baseHappyClip;
    private AnimationClip baseHungryClip;
    private AnimationClip baseLyingClip;
    private AnimationClip baseSadClip;
    private AnimationClip baseSleepClip;
    private AnimationClip baseStretchClip;
    private ThePet pet;
    private ThePetStatsManager statsManager;
    private PetSkinUnlockService unlockService;
    private bool saveSelectionEnabled;

    [Serializable]
    private class SelectedSkinSaveData
    {
        public int selectedSkinIndex;
    }

    private void Awake()
    {
        pet = GetComponent<ThePet>();
        statsManager = GetComponent<ThePetStatsManager>();

        if (animatorComponent == null)
        {
            animatorComponent = GetComponent<Animator>();
        }

        if (skinLibrary == null)
        {
            skinLibrary = Resources.Load<PetSkinLibrary>(DefaultLibraryResourcePath);
        }

        if (animatorComponent == null || skinLibrary == null)
        {
            return;
        }

        baseController = animatorComponent.runtimeAnimatorController;
        baseIdleClip = FindBaseClip(baseController, BaseIdleClipName);
        baseHappyClip = FindBaseClip(baseController, BaseHappyClipName);
        baseHungryClip = FindBaseClip(baseController, BaseHungryClipName);
        baseLyingClip = FindBaseClip(baseController, BaseLyingClipName);
        baseSadClip = FindBaseClip(baseController, BaseSadClipName);
        baseSleepClip = FindBaseClip(baseController, BaseSleepClipName);
        baseStretchClip = FindBaseClip(baseController, BaseStretchClipName);

        if (baseController != null &&
            baseIdleClip != null &&
            baseHappyClip != null &&
            baseHungryClip != null &&
            baseLyingClip != null &&
            baseSadClip != null &&
            baseSleepClip != null &&
            baseStretchClip != null)
        {
            overrideController = new AnimatorOverrideController(baseController);
            animatorComponent.runtimeAnimatorController = overrideController;
            ApplySkin(selectedSkinIndex);
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        saveSelectionEnabled = true;

        int savedSkinIndex = LoadSavedSkinIndex();
        int unlockedSkinIndex = GetUnlockedSelectedSkinIndex(savedSkinIndex);
        if (unlockedSkinIndex != selectedSkinIndex)
        {
            ApplySkin(unlockedSkinIndex);
        }
    }

    public int GetSkinCount()
    {
        return skinLibrary != null ? skinLibrary.Count : 0;
    }

    public int GetSelectedSkinIndex()
    {
        return selectedSkinIndex;
    }

    public string GetSkinDisplayName(int index)
    {
        return skinLibrary != null ? skinLibrary.GetDisplayName(index) : string.Empty;
    }

    public Sprite GetSkinPreviewSprite(int index)
    {
        return skinLibrary != null ? skinLibrary.GetPreviewSprite(index) : null;
    }

    public bool IsSkinUnlocked(int index)
    {
        if (skinLibrary == null || index < 0 || index >= skinLibrary.Count)
        {
            return false;
        }

        return GetUnlockService().IsUnlocked(skinLibrary.GetUnlockCondition(index));
    }

    public string GetSkinUnlockDescription(int index)
    {
        return skinLibrary != null ? skinLibrary.GetUnlockDescription(index) : string.Empty;
    }

    public string GetSkinUnlockProgressText(int index)
    {
        if (skinLibrary == null || index < 0 || index >= skinLibrary.Count)
        {
            return string.Empty;
        }

        return GetUnlockService().GetProgressText(skinLibrary.GetUnlockCondition(index));
    }

    public bool ApplySkin(int index)
    {
        if (skinLibrary == null ||
            overrideController == null ||
            baseIdleClip == null ||
            baseHappyClip == null ||
            baseHungryClip == null ||
            baseLyingClip == null ||
            baseSadClip == null ||
            baseSleepClip == null ||
            baseStretchClip == null ||
            animatorComponent == null)
        {
            return false;
        }

        int skinCount = skinLibrary.Count;
        if (skinCount <= 0)
        {
            return false;
        }

        int clampedIndex = Mathf.Clamp(index, 0, skinCount - 1);
        if (!IsSkinUnlocked(clampedIndex))
        {
            return false;
        }

        AnimationClip selectedIdleClip = skinLibrary.GetIdleClip(clampedIndex);
        AnimationClip selectedHappyClip = skinLibrary.GetHappyClip(clampedIndex);
        AnimationClip selectedHungryClip = skinLibrary.GetHungryClip(clampedIndex);
        AnimationClip selectedLyingClip = skinLibrary.GetLyingClip(clampedIndex);
        AnimationClip selectedSadClip = skinLibrary.GetSadClip(clampedIndex);
        AnimationClip selectedSleepClip = skinLibrary.GetSleepClip(clampedIndex);
        AnimationClip selectedStretchClip = skinLibrary.GetStretchClip(clampedIndex);
        if (selectedIdleClip == null ||
            selectedHappyClip == null ||
            selectedHungryClip == null ||
            selectedLyingClip == null ||
            selectedSadClip == null ||
            selectedSleepClip == null ||
            selectedStretchClip == null)
        {
            return false;
        }

        overrideController[BaseIdleClipName] = selectedIdleClip;
        overrideController[BaseHappyClipName] = selectedHappyClip;
        overrideController[BaseHungryClipName] = selectedHungryClip;
        overrideController[BaseLyingClipName] = selectedLyingClip;
        overrideController[BaseSadClipName] = selectedSadClip;
        overrideController[BaseSleepClipName] = selectedSleepClip;
        overrideController[BaseStretchClipName] = selectedStretchClip;
        animatorComponent.runtimeAnimatorController = overrideController;
        selectedSkinIndex = clampedIndex;
        if (saveSelectionEnabled)
        {
            SaveSelectedSkinIndex();
        }

        if (pet != null && pet.states != null && pet.states.current != null)
        {
            Type currentStateType = pet.states.current.GetType();
            if (currentStateType == typeof(IdleState))
            {
                animatorComponent.Play(IdleStateName, 0, 0f);
            }
            else if (currentStateType == typeof(HappyState))
            {
                animatorComponent.Play(HappyStateName, 0, 0f);
            }
            else if (currentStateType == typeof(HungryState))
            {
                animatorComponent.Play(HungryStateName, 0, 0f);
            }
            else if (currentStateType == typeof(LyingState))
            {
                animatorComponent.Play(LyingStateName, 0, 0f);
            }
            else if (currentStateType == typeof(SadState))
            {
                animatorComponent.Play(SadStateName, 0, 0f);
            }
            else if (currentStateType == typeof(SleepState))
            {
                animatorComponent.Play(SleepStateName, 0, 0f);
            }
            else if (currentStateType == typeof(StretchState))
            {
                animatorComponent.Play(StretchStateName, 0, 0f);
            }
        }

        return true;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveSelectedSkinIndex();
        }
    }

    private void OnApplicationQuit()
    {
        SaveSelectedSkinIndex();
    }

    private PetSkinUnlockService GetUnlockService()
    {
        if (unlockService == null)
        {
            unlockService = new PetSkinUnlockService(statsManager);
        }

        return unlockService;
    }

    private int GetUnlockedSelectedSkinIndex(int requestedIndex)
    {
        int skinCount = skinLibrary != null ? skinLibrary.Count : 0;
        if (skinCount <= 0)
        {
            return 0;
        }

        int clampedIndex = Mathf.Clamp(requestedIndex, 0, skinCount - 1);
        return IsSkinUnlocked(clampedIndex) ? clampedIndex : 0;
    }

    private static AnimationClip FindBaseClip(RuntimeAnimatorController controller, string clipName)
    {
        if (controller == null)
        {
            return null;
        }

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip != null && clip.name == clipName)
            {
                return clip;
            }
        }

        return null;
    }

    private int LoadSavedSkinIndex()
    {
        string savePath = GetSelectedSkinSavePath();
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                SelectedSkinSaveData data = JsonUtility.FromJson<SelectedSkinSaveData>(json);
                if (data != null)
                {
                    return data.selectedSkinIndex;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"PetSkinManager: failed to load selected skin. {exception.Message}");
            }
        }

        return selectedSkinIndex;
    }

    private void SaveSelectedSkinIndex()
    {
        if (!saveSelectionEnabled)
        {
            return;
        }

        try
        {
            string savePath = GetSelectedSkinSavePath();
            string directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            SelectedSkinSaveData data = new()
            {
                selectedSkinIndex = selectedSkinIndex
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"PetSkinManager: failed to save selected skin. {exception.Message}");
        }
    }

    private static string GetSelectedSkinSavePath()
    {
        return Path.Combine(AccountPathProvider.GetCurrentAccountRoot(), "selected-skin.json");
    }
}
