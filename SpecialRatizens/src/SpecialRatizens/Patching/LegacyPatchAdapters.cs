using System;
using System.Collections.Generic;
using CasselGames.Diplomatic.Data;
using CasselGames.Diplomatic.UI;
using RatopiaMod;

namespace SpecialRatizens.Patching
{
    /// <summary>
    /// 将原作者的特性公式包在故障隔离边界内。Prefix 失败时继续原版，Postfix 失败时停止本次附加逻辑。
    /// </summary>
    internal static class LegacyPatchAdapters
    {
        public static void DB_Mgr_Character_DB_Setting(DB_Mgr __instance)
        {
            Run("data.character-db", () => CustomMOD.DB_Mgr_Character_DB_Setting(__instance));
        }

        public static void CitizenCaveUI_MakeCitizenList()
        {
            Run("generation.list", CustomMOD.CitizenCaveUI_MakeCitizenList);
        }

        public static bool CCMake_Info(CCMake_Info __instance, int _grade_max, bool _religion_check = false)
        {
            try { return CustomMOD.CCMake_Info(__instance, _grade_max, _religion_check); }
            catch (Exception error) { return FailOpen("generation.candidate-constructor", error); }
        }

        public static bool CCMake_Info_MakeCharacterList(CCMake_Info __instance)
        {
            try { return CustomMOD.CCMake_Info_MakeCharacterList(__instance); }
            catch (Exception error) { return FailOpen("generation.default-trait-boundary", error); }
        }

        public static void T_Citizen_MakeCtizen_ByCC(T_Citizen __instance, CCMake_Info _info)
        {
            Run("generation.citizen-created", () => CustomMOD.T_Citizen_MakeCtizen_ByCC(__instance, _info));
        }

        public static void GBot_MakeCitizen(GBot __instance, int _index)
        {
            Run("power.robot-created", () => CustomMOD.GBot_MakeCitizen(__instance, _index));
        }

        public static bool GBot_FatigueUpate(GBot __instance, float value)
        {
            try { return CustomMOD.GBot_FatigueUpate(__instance, value); }
            catch (Exception error) { return FailOpen("power.robot-fatigue", error); }
        }

        public static bool ElecLine_Info_AddConnectUseBuild(ElecLine_Info __instance, int _id, float _value)
        {
            try { return CustomMOD.ElecLine_Info_AddConnectUseBuild(__instance, _id, _value); }
            catch (Exception error) { return FailOpen("power.connect-building", error); }
        }

        public static void ElecLine_Info_AddWatt(ElecLine_Info __instance, float _value)
        {
            Run("power.add-watt", () => CustomMOD.ElecLine_Info_AddWatt(__instance, _value));
        }

        public static void Building_WireCheck(Building __instance, bool _use, ref bool __result)
        {
            try { CustomMOD.Building_WireCheck(__instance, _use, ref __result); }
            catch (Exception error) { Plugin.LogPatchError("power.wire-check-building", error); }
        }

        public static void Building_ElecMasonry_WireCheck(Building_ElecMasonry __instance, bool _use, ref bool __result)
        {
            try { CustomMOD.Building_ElecMasonry_WireCheck(__instance, _use, ref __result); }
            catch (Exception error) { Plugin.LogPatchError("power.wire-check-masonry", error); }
        }

        public static void Building_ElecCarrierStation_WireCheck(Building_ElecCarrierStation __instance, bool _use, ref bool __result)
        {
            try { CustomMOD.Building_ElecCarrierStation_WireCheck(__instance, _use, ref __result); }
            catch (Exception error) { Plugin.LogPatchError("power.wire-check-carrier", error); }
        }

        public static void Building_ElecBandstand_WireCheck(Building_ElecBandstand __instance, bool _use, ref bool __result)
        {
            try { CustomMOD.Building_ElecBandstand_WireCheck(__instance, _use, ref __result); }
            catch (Exception error) { Plugin.LogPatchError("power.wire-check-bandstand", error); }
        }

        public static bool BuildingMgr_GetFourDir_ElecGroup(ElecPort _port, ref List<ElecLine_Info> __result)
        {
            try { return CustomMOD.BuildingMgr_GetFourDir_ElecGroup(_port, ref __result); }
            catch (Exception error) { return FailOpen("power.four-direction-grid", error); }
        }

        public static bool BuildingMgr_DeleteConnectCheck(BuildingMgr __instance, int _id, List<ElecPort> _list_port)
        {
            try { return CustomMOD.BuildingMgr_DeleteConnectCheck(__instance, _id, _list_port); }
            catch (Exception error) { return FailOpen("power.delete-connect", error); }
        }

        public static void ElecLine_Info_UseWatt(ElecLine_Info __instance, ref float _value)
        {
            try { CustomMOD.ElecLine_Info_UseWatt(__instance, ref _value); }
            catch (Exception error) { Plugin.LogPatchError("power.quantum-grid", error); }
        }

        public static void MasonryInfo_WorkUpdate_Prefix(MasonryInfo __instance, ref float d_time)
        {
            try { CustomMOD.MasonryInfo_WorkUpdate_Prefix(__instance, ref d_time); }
            catch (Exception error) { Plugin.LogPatchError("industry.work-prefix", error); }
        }

        public static void MasonryInfo_WorkUpdate_Postfix(MasonryInfo __instance, ref float d_time)
        {
            try { CustomMOD.MasonryInfo_WorkUpdate_Postfix(__instance, ref d_time); }
            catch (Exception error) { Plugin.LogPatchError("industry.work-postfix", error); }
        }

        public static void T_Citizen_ApplyFoodOrLife_ResAbility(T_Citizen __instance, TileInfo t_info)
        {
            Run("industry.food-life", () => CustomMOD.T_Citizen_ApplyFoodOrLife_ResAbility(__instance, t_info));
        }

        public static bool Helpers_Get_MaximumGuestNum(BuildingName _name, ref int __result)
        {
            try { return CustomMOD.Helpers_Get_MaximumGuestNum(_name, ref __result); }
            catch (Exception error) { return FailOpen("industry.guest-capacity", error); }
        }

        public static bool DiplomaticCountryResourceData_TradeCountryToMyKingdomPrice(
            float price, int nowRelations, TileInfo ____info, ref float __result)
        {
            try { return CustomMOD.DiplomaticCountryResourceData_TradeCountryToMyKingdomPrice(price, nowRelations, ____info, ref __result); }
            catch (Exception error) { return FailOpen("economy.import-price", error); }
        }

        public static bool DiplomaticCountryResourceData_TradeMyKingdomToCountryPrice(
            float price, int nowRelations, TileInfo ____info, ref float __result)
        {
            try { return CustomMOD.DiplomaticCountryResourceData_TradeMyKingdomToCountryPrice(price, nowRelations, ____info, ref __result); }
            catch (Exception error) { return FailOpen("economy.export-price", error); }
        }

        public static void DiplomaticMgr_OnTradeResultEvent_BGNYQY(TradeResult result, TradeReceive __result)
        {
            Run("economy.trade-result", () => CustomMOD.DiplomaticMgr_OnTradeResultEvent_BGNYQY(result, __result));
        }

        public static void DiplomaticData_SetTerrainTotalDistance(DiplomaticWorldTerrainEntity tInstance)
        {
            Run("economy.distance", () => CustomMOD.DiplomaticData_SetTerrainTotalDistance(tInstance));
        }

        public static bool DiplomaticCountryData_MaxTradeAgreementCount(ref int __result)
        {
            try { return CustomMOD.DiplomaticCountryData_MaxTradeAgreementCount(ref __result); }
            catch (Exception error) { return FailOpen("economy.agreement-count", error); }
        }

        public static void DiplomaticTradeSheetDetailContentsUI_SetData(
            DiplomaticTradeSheetDetailContentsUI __instance,
            DiplomaticCountryData cData,
            DiplomaticCountryTradeSheetData sData,
            TypeTradeSheet typeTradeSheet,
            List<DiplomaticTradeSheetDetailInfoUI> ____infoList)
        {
            Run("economy.detail-price", () =>
                CustomMOD.DiplomaticTradeSheetDetailContentsUI_SetData(__instance, cData, sData, typeTradeSheet, ____infoList));
        }

        public static void T_Citizen_JobSet(T_Citizen __instance)
        {
            Run("citizen.job", () => CustomMOD.T_Citizen_JobSet(__instance));
        }

        public static void T_Citizen_SwdAtk_Call(T_Citizen __instance)
        {
            Run("combat.sword-attack", () => CustomMOD.T_Citizen_SwdAtk_Call(__instance));
        }

        public static bool T_Citizen_BeAttacked(T_Citizen __instance, ref float dmg, Unit_Attacekd_Tag _tag)
        {
            try { return CustomMOD.T_Citizen_BeAttacked(__instance, ref dmg, _tag); }
            catch (Exception error) { return FailOpen("combat.citizen-attacked", error); }
        }

        public static void FoodUI_AllFood_Update()
        {
            Run("state.food-total", CustomMOD.FoodUI_AllFood_Update);
        }

        public static void GameUnit_UpdatePDI_Post()
        {
            Run("state.pdi", CustomMOD.GameUnit_UpdatePDI_Post);
        }

        public static void T_Citizen_HungerUpdate(T_Citizen __instance, float value)
        {
            Run("state.hunger", () => CustomMOD.T_Citizen_HungerUpdate(__instance, value));
        }

        public static void BuffIcon_IconSet(BuffIcon __instance, BuffInfo _info)
        {
            Run("state.buff-icon", () => CustomMOD.BuffIcon_IconSet(__instance, _info));
        }

        public static bool RefInfo_GetIconAddress(string _RefName, ref string __result)
        {
            try { return CustomMOD.RefInfo_GetIconAddress(_RefName, ref __result); }
            catch (Exception error) { return FailOpen("state.icon-address", error); }
        }

        public static bool RefInfo_Get_T_Name(string _RefName, ref string __result)
        {
            try { return CustomMOD.RefInfo_Get_T_Name(_RefName, ref __result); }
            catch (Exception error) { return FailOpen("state.display-name", error); }
        }

        public static bool CitizenBuff_RefInfo_GetDescript(CitizenBuff.RefInfo __instance, ref string __result)
        {
            try { return CustomMOD.CitizenBuff_RefInfo_GetDescript(__instance, ref __result); }
            catch (Exception error) { return FailOpen("state.description", error); }
        }

        public static bool T_Citizen_DefaultClothesUpdate(T_Citizen __instance)
        {
            try { return CustomMOD.T_Citizen_DefaultClothesUpdate(__instance); }
            catch (Exception error) { return FailOpen("appearance.default-clothes", error); }
        }

        public static bool GameUnit_ClothesUpdate(GameUnit __instance, int num)
        {
            try { return CustomMOD.GameUnit_ClothesUpdate(__instance, num); }
            catch (Exception error) { return FailOpen("appearance.work-clothes", error); }
        }

        private static void Run(string operation, Action action)
        {
            try { action(); }
            catch (Exception error) { Plugin.LogPatchError(operation, error); }
        }

        private static bool FailOpen(string operation, Exception error)
        {
            Plugin.LogPatchError(operation, error);
            return true;
        }
    }
}
