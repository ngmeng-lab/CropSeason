using System.Reflection;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace CropSeason
{
    public class ModEntry : Mod
    {
        internal static IMonitor? SMonitor;

        public override void Entry(IModHelper helper)
        {
            SMonitor = this.Monitor;

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded; // 存档加载时先修改
            helper.Events.GameLoop.Saved += OnSaved;         // 保存后再次修改

            SMonitor!.Log("成功应用 Crop 四季生长补丁", LogLevel.Info);
        }

        private void OnSaved(object sender, EventArgs e)
        {
            SetAllPlantedCropsSeasonsToAll();
        }

        private void OnSaveLoaded(object? sender, EventArgs e)
        {
            SetAllPlantedCropsSeasonsToAll();
        }

        private static void SetAllPlantedCropsSeasonsToAll()
        {
            foreach (var location in Game1.locations)
            {
                foreach (var pair in location.terrainFeatures.Pairs)
                {
                    if (pair.Value is HoeDirt dirt && dirt.crop != null)
                    {
                        try
                        {
                            // 修改作物季节
                            var getDataMethod = typeof(Crop).GetMethod("GetData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (getDataMethod != null)
                            {
                                var cropData = getDataMethod.Invoke(dirt.crop, null);
                                if (cropData != null)
                                {
                                    var seasonsField = cropData.GetType().GetField("Seasons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (seasonsField != null)
                                    {
                                        var allSeasons = new List<Season>
                                        {
                                            Season.Spring,
                                            Season.Summer,
                                            Season.Fall,
                                            Season.Winter
                                        };
                                        seasonsField.SetValue(cropData, allSeasons);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SMonitor!.Log($"修改作物季节或复活作物失败: {ex}", LogLevel.Warn);
                        }
                    }
                }
            }
        }
    }
}
