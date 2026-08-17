using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ScaffoldMod.Runtime
{
    internal static class ScaffoldAssets
    {
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        private static readonly HashSet<string> Warned = new HashSet<string>();

        internal static Sprite World => Load("world.png", "GameScene/Map/Building/Building_Ladder");

        internal static Sprite Menu => Load("menu.png", "GameScene/Map/Building/Building_Ladder");

        internal static Sprite Blueprint => Load("blueprint.png", "GameScene/Map/Building/BluePrint/Building_Ladder");

        internal static void Clear()
        {
            Sprites.Clear();
            Warned.Clear();
        }

        private static Sprite Load(string fileName, string fallbackPath)
        {
            if (Sprites.TryGetValue(fileName, out var cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var dataDirectory = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                    "Data");
                var path = Path.Combine(dataDirectory, fileName);
                if (File.Exists(path))
                {
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        name = "Scaffold_" + fileName
                    };
                    if (texture.LoadImage(File.ReadAllBytes(path), false))
                    {
                        var sprite = Sprite.Create(
                            texture,
                            new Rect(0f, 0f, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f),
                            texture.width);
                        sprite.name = "Scaffold_" + Path.GetFileNameWithoutExtension(fileName);
                        Sprites[fileName] = sprite;
                        return sprite;
                    }
                }
            }
            catch (Exception exception)
            {
                ScaffoldRuntime.LogWarning($"载入素材 {fileName} 失败：{exception.Message}");
            }

            if (Warned.Add(fileName))
            {
                ScaffoldRuntime.LogWarning($"缺少素材 {fileName}，回退到原版梯子图像。");
            }

            return Func.Instance?.LoadSprite(fallbackPath);
        }
    }
}
