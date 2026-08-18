namespace EquipmentReforgeSelector
{
    public readonly struct InlineCandidateRow
    {
        public InlineCandidateRow(int candidateIndex, ReforgeCandidate candidate, bool isSelected)
        {
            CandidateIndex = candidateIndex;
            Candidate = candidate;
            IsSelected = isSelected;
        }

        public int CandidateIndex { get; }

        public ReforgeCandidate Candidate { get; }

        public bool IsSelected { get; }
    }
}
