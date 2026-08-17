using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using BepInEx.Logging;
using CasselGames.Diplomatic;
using CasselGames.Diplomatic.Data;
using CasselGames.Diplomatic.UI;
using HarmonyLib;
using RatopiaMod;
using UnityEngine;

namespace SpecialRatizens.Patching
{
    internal static class PatchRegistry
    {
        private static readonly IReadOnlyList<PatchDescriptor> Items =
            new ReadOnlyCollection<PatchDescriptor>(BuildDescriptors());

        public static IReadOnlyList<PatchDescriptor> Descriptors => Items;

        public static void InstallAll(Harmony harmony, ManualLogSource logger)
        {
            if (harmony == null)
            {
                throw new ArgumentNullException(nameof(harmony));
            }

            foreach (var descriptor in Items)
            {
                logger.LogDebug($"正在安装 Harmony 补丁：{descriptor.Name}");
                descriptor.Apply(harmony);
                logger.LogDebug($"Harmony 补丁安装完成：{descriptor.Name}");
            }
        }

        private static List<PatchDescriptor> BuildDescriptors()
        {
            var descriptors = new List<PatchDescriptor>
            {
                Postfix("data.character-db", () => Method(typeof(DB_Mgr), "Character_DB_Setting"), "DB_Mgr_Character_DB_Setting"),
                Postfix("session.loaded", () => Method(typeof(TileMgr), "All_NotUseListClear"), typeof(SessionPatches), "TileMgrAllNotUseListClearPostfix"),
                Prefix("generation.list", () => Method(typeof(CitizenCaveUI), "MakeCitizenList"), "CitizenCaveUI_MakeCitizenList"),
                Prefix("generation.candidate-constructor", () => AccessTools.Constructor(typeof(CCMake_Info), new[] { typeof(int), typeof(bool) }), "CCMake_Info"),
                Prefix("generation.default-trait-boundary", () => Method(typeof(CCMake_Info), "MakeCharacterList"), "CCMake_Info_MakeCharacterList"),
                Postfix("generation.citizen-created", () => Method(typeof(T_Citizen), "MakeCtizen_ByCC", typeof(Vector2), typeof(CCMake_Info)), "T_Citizen_MakeCtizen_ByCC"),

                Postfix("power.robot-created", () => Method(typeof(GBot), "MakeCitizen", typeof(Vector2), typeof(int)), "GBot_MakeCitizen"),
                Prefix("power.robot-fatigue", () => Method(typeof(GBot), "FatigueUpate", typeof(float), typeof(bool)), "GBot_FatigueUpate"),
                Prefix("power.connect-building", () => Method(typeof(ElecLine_Info), "AddConnectUseBuild", typeof(int), typeof(float)), "ElecLine_Info_AddConnectUseBuild"),
                Postfix("power.add-watt", () => Method(typeof(ElecLine_Info), "AddWatt", typeof(float)), "ElecLine_Info_AddWatt"),
                Postfix("power.wire-check-building", () => Method(typeof(Building), "WireCheck", typeof(bool)), "Building_WireCheck"),
                Postfix("power.wire-check-masonry", () => Method(typeof(Building_ElecMasonry), "WireCheck", typeof(bool)), "Building_ElecMasonry_WireCheck"),
                Postfix("power.wire-check-carrier", () => Method(typeof(Building_ElecCarrierStation), "WireCheck", typeof(bool)), "Building_ElecCarrierStation_WireCheck"),
                Postfix("power.wire-check-bandstand", () => Method(typeof(Building_ElecBandstand), "WireCheck", typeof(bool)), "Building_ElecBandstand_WireCheck"),
                Prefix("power.four-direction-grid", () => Method(typeof(BuildingMgr), "GetFourDir_ElecGroup", typeof(ElecPort), typeof(bool)), "BuildingMgr_GetFourDir_ElecGroup"),
                Prefix("power.delete-connect", () => Method(typeof(BuildingMgr), "DeleteConnectCheck", typeof(int), typeof(List<ElecPort>)), "BuildingMgr_DeleteConnectCheck"),
                Prefix("power.quantum-grid", () => Method(typeof(ElecLine_Info), "UseWatt", typeof(int), typeof(float)), "ElecLine_Info_UseWatt"),

                Prefix("industry.work-prefix", () => Method(typeof(MasonryInfo), "WorkUpdate", typeof(float)), "MasonryInfo_WorkUpdate_Prefix"),
                Postfix("industry.work-postfix", () => Method(typeof(MasonryInfo), "WorkUpdate", typeof(float)), "MasonryInfo_WorkUpdate_Postfix"),
                Postfix("industry.food-life", () => Method(typeof(T_Citizen), "ApplyFoodOrLife_ResAbility", typeof(TileInfo)), "T_Citizen_ApplyFoodOrLife_ResAbility"),
                Prefix("industry.guest-capacity", () => Method(typeof(Helpers), "Get_MaximumGuestNum", typeof(BuildingName)), "Helpers_Get_MaximumGuestNum"),

                Prefix("economy.import-price", () => Method(typeof(DiplomaticCountryResourceData), "TradeCountryToMyKingdomPrice", typeof(float), typeof(int)), "DiplomaticCountryResourceData_TradeCountryToMyKingdomPrice"),
                Prefix("economy.export-price", () => Method(typeof(DiplomaticCountryResourceData), "TradeMyKingdomToCountryPrice", typeof(float), typeof(int)), "DiplomaticCountryResourceData_TradeMyKingdomToCountryPrice"),
                Postfix("economy.trade-result", () => Method(typeof(DiplomaticMgr), "OnTradeResultEvent", typeof(TradeResult)), "DiplomaticMgr_OnTradeResultEvent_BGNYQY"),
                Postfix("economy.distance", () => Method(typeof(DiplomaticData), "SetTerrainTotalDistance", typeof(DiplomaticWorldTerrainEntity)), "DiplomaticData_SetTerrainTotalDistance"),
                Prefix("economy.agreement-count", () => AccessTools.PropertyGetter(typeof(DiplomaticCountryData), "MaxTradeAgreementCount"), "DiplomaticCountryData_MaxTradeAgreementCount"),
                Postfix("economy.detail-price", () => Method(typeof(DiplomaticTradeSheetDetailContentsUI), "SetData", typeof(DiplomaticCountryData), typeof(DiplomaticCountryTradeSheetData), typeof(TypeTradeSheetCategory), typeof(TypeTradeSheet)), "DiplomaticTradeSheetDetailContentsUI_SetData"),

                Postfix("citizen.job", () => Method(typeof(T_Citizen), "JobSet", typeof(Building)), "T_Citizen_JobSet"),
                Prefix("combat.sword-attack", () => Method(typeof(T_Citizen), "SwdAtk_Call"), "T_Citizen_SwdAtk_Call"),
                Prefix("combat.citizen-attacked", () => Method(typeof(T_Citizen), "BeAttacked", typeof(float), typeof(Unit_Attacekd_Tag), typeof(int)), "T_Citizen_BeAttacked"),
                Postfix("state.food-total", () => Method(typeof(FoodUI), "AllFood_Update"), "FoodUI_AllFood_Update"),
                Postfix("state.pdi", () => Method(typeof(GameUnit), "UpdatePDI", typeof(PDI), typeof(float)), "GameUnit_UpdatePDI_Post", Type.EmptyTypes),
                Postfix("state.hunger", () => Method(typeof(T_Citizen), "HungerUpdate", typeof(float)), "T_Citizen_HungerUpdate"),
                Postfix("state.buff-icon", () => Method(typeof(BuffIcon), "IconSet", typeof(Transform), typeof(BuffInfo)), "BuffIcon_IconSet"),
                Prefix("state.icon-address", () => Method(typeof(CitizenBuff.RefInfo), "GetIconAddress", typeof(string), typeof(C_Buff_Category)), "RefInfo_GetIconAddress"),
                Prefix("state.display-name", () => Method(typeof(CitizenBuff.RefInfo), "Get_T_Name", typeof(string), typeof(C_Buff_Category), typeof(bool)), "RefInfo_Get_T_Name"),
                Prefix("state.description", () => Method(typeof(CitizenBuff.RefInfo), "GetDescript"), "CitizenBuff_RefInfo_GetDescript"),

                Prefix("appearance.default-clothes", () => Method(typeof(T_Citizen), "DefaultClothesUpdate"), "T_Citizen_DefaultClothesUpdate"),
                Prefix("appearance.work-clothes", () => Method(typeof(GameUnit), "ClothesUpdate", typeof(int), typeof(bool)), "GameUnit_ClothesUpdate")
            };
            return descriptors;
        }

        private static PatchDescriptor Prefix(string name, Func<MethodBase> target, string patchName, Type[] patchParameters = null)
        {
            return Create(name, PatchKind.Prefix, target, typeof(LegacyPatchAdapters), patchName, patchParameters);
        }

        private static PatchDescriptor Postfix(string name, Func<MethodBase> target, string patchName, Type[] patchParameters = null)
        {
            return Create(name, PatchKind.Postfix, target, typeof(LegacyPatchAdapters), patchName, patchParameters);
        }

        private static PatchDescriptor Postfix(string name, Func<MethodBase> target, Type patchType, string patchName)
        {
            return Create(name, PatchKind.Postfix, target, patchType, patchName, Type.EmptyTypes);
        }

        private static PatchDescriptor Create(
            string name,
            PatchKind kind,
            Func<MethodBase> target,
            Type patchType,
            string patchName,
            Type[] patchParameters)
        {
            var method = patchParameters == null
                ? AccessTools.Method(patchType, patchName)
                : AccessTools.Method(patchType, patchName, patchParameters);
            if (method == null)
            {
                throw new MissingMethodException(patchType.FullName, patchName);
            }
            return new PatchDescriptor(name, kind, target, method);
        }

        private static MethodInfo Method(Type type, string name, params Type[] parameters)
        {
            return AccessTools.Method(type, name, parameters);
        }
    }
}
