namespace RestroomBathFun.Core
{
    internal readonly struct ServiceCompletionState
    {
        internal ServiceCompletionState(FacilityKind facility, bool serviceAborted)
        {
            Facility = facility;
            ServiceAborted = serviceAborted;
        }

        internal FacilityKind Facility { get; }

        internal bool ServiceAborted { get; }
    }
}
