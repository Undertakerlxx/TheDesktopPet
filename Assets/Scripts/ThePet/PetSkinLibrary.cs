using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PetSkinLibrary", menuName = "DesktopPet/Pet Skin Library")]
public class PetSkinLibrary : ScriptableObject
{
    [Serializable]
    public class SkinDefinition
    {
        public string displayName;
        public Sprite previewSprite;
        public AnimationClip idleClip;
        public AnimationClip happyClip;
        public AnimationClip hungryClip;
        public AnimationClip lyingClip;
        public AnimationClip sadClip;
        public AnimationClip sleepClip;
        public AnimationClip stretchClip;
    }

    [SerializeField] private List<SkinDefinition> skins = new();

    public int Count => skins.Count;

    public SkinDefinition GetSkin(int index)
    {
        if (index < 0 || index >= skins.Count)
        {
            return null;
        }

        return skins[index];
    }

    public string GetDisplayName(int index)
    {
        return GetSkin(index)?.displayName ?? string.Empty;
    }

    public Sprite GetPreviewSprite(int index)
    {
        return GetSkin(index)?.previewSprite;
    }

    public AnimationClip GetIdleClip(int index)
    {
        return GetSkin(index)?.idleClip;
    }

    public AnimationClip GetHappyClip(int index)
    {
        return GetSkin(index)?.happyClip;
    }

    public AnimationClip GetHungryClip(int index)
    {
        return GetSkin(index)?.hungryClip;
    }

    public AnimationClip GetLyingClip(int index)
    {
        return GetSkin(index)?.lyingClip;
    }

    public AnimationClip GetSadClip(int index)
    {
        return GetSkin(index)?.sadClip;
    }

    public AnimationClip GetSleepClip(int index)
    {
        return GetSkin(index)?.sleepClip;
    }

    public AnimationClip GetStretchClip(int index)
    {
        return GetSkin(index)?.stretchClip;
    }
}
