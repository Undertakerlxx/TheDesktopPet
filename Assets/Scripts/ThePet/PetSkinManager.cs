using System;
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

    private void Awake()
    {
        pet = GetComponent<ThePet>();

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
}
