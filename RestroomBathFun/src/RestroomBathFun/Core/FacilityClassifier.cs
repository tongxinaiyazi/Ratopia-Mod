namespace RestroomBathFun.Core
{
    internal static class FacilityClassifier
    {
        private const int ToiletBuildingName = 110;
        private const int BathsBuildingName = 114;

        internal static FacilityKind Classify(int buildingName)
        {
            switch (buildingName)
            {
                case ToiletBuildingName:
                    return FacilityKind.Toilet;
                case BathsBuildingName:
                    return FacilityKind.Baths;
                default:
                    return FacilityKind.Unsupported;
            }
        }
    }
}
