using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    public partial class EyeHandSpeedContentController
    {
        private void ApplyTileVisual(int index, EyeHandTileData tileData)
        {
            Image image = cellImages[index];
            Text label = cellLabels[index];
            image.color = tileData.Role == EyeHandTileRole.Bomb ? new Color(0.2f, 0.2f, 0.22f, 0.98f) : tileData.Color;
            label.text = tileData.Symbol;
            MiniGameUiFactory.StyleSymbolText(
                label,
                tileData.Role == EyeHandTileRole.Bomb ? 26 : 34,
                tileData.Role == EyeHandTileRole.Bomb ? new Color(1f, 0.82f, 0.82f) : new Color(0.10f, 0.10f, 0.10f),
                tileData.Role == EyeHandTileRole.Bomb ? new Color(0f, 0f, 0f, 0.45f) : new Color(1f, 1f, 1f, 0.55f));
        }

        private EyeHandTargetSpec RandomTarget()
        {
            EyeHandShapeSpec shape = shapes[Random.Range(0, shapes.Length)];
            EyeHandNamedColor color = palette[Random.Range(0, palette.Length)];
            return new EyeHandTargetSpec(shape.Symbol, shape.Name, color.Value, color.Name);
        }

        private EyeHandTargetSpec RandomDecoy(EyeHandTargetSpec forbidden)
        {
            EyeHandTargetSpec target;
            do { target = RandomTarget(); }
            while (target.Symbol == forbidden.Symbol && target.ColorName == forbidden.ColorName);
            return target;
        }

        private static int TakeRandomIndex(List<int> pool)
        {
            int pick = Random.Range(0, pool.Count);
            int value = pool[pick];
            pool.RemoveAt(pick);
            return value;
        }
    }

    internal readonly struct EyeHandShapeSpec
    {
        public EyeHandShapeSpec(string symbol, string name) { Symbol = symbol; Name = name; }
        public string Symbol { get; }
        public string Name { get; }
    }

    internal readonly struct EyeHandNamedColor
    {
        public EyeHandNamedColor(Color value, string name) { Value = value; Name = name; }
        public Color Value { get; }
        public string Name { get; }
    }

    internal readonly struct EyeHandTargetSpec
    {
        public EyeHandTargetSpec(string symbol, string shapeName, Color color, string colorName)
        {
            Symbol = symbol;
            ShapeName = shapeName;
            Color = color;
            ColorName = colorName;
        }

        public string Symbol { get; }
        public string ShapeName { get; }
        public Color Color { get; }
        public string ColorName { get; }
    }

    internal readonly struct EyeHandTileData
    {
        public EyeHandTileData(EyeHandTileRole role, string symbol, Color color) { Role = role; Symbol = symbol; Color = color; }
        public EyeHandTileRole Role { get; }
        public string Symbol { get; }
        public Color Color { get; }
        public static EyeHandTileData Target(EyeHandTargetSpec target) => new(EyeHandTileRole.Target, target.Symbol, target.Color);
        public static EyeHandTileData Decoy(EyeHandTargetSpec target) => new(EyeHandTileRole.Decoy, target.Symbol, target.Color);
        public static EyeHandTileData Bomb() => new(EyeHandTileRole.Bomb, "BOMB", Color.black);
    }

    internal enum EyeHandTileRole { Target, Bomb, Decoy }
}
