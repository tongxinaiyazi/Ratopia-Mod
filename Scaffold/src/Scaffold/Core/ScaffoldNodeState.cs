namespace ScaffoldMod.Core
{
    internal sealed class ScaffoldNodeState
    {
        internal const int LadderNodeType = 3;

        internal ScaffoldNodeState(int underlyingNodeType)
        {
            UnderlyingNodeType = underlyingNodeType;
        }

        internal int UnderlyingNodeType { get; private set; }

        internal int OverlayNodeType => LadderNodeType;

        internal void CaptureRebuiltUnderlyingNode(int nodeType)
        {
            UnderlyingNodeType = nodeType;
        }

        internal void CaptureRuntimeNode(int nodeType)
        {
            if (nodeType != LadderNodeType)
            {
                UnderlyingNodeType = nodeType;
            }
        }

        internal int RestoreNodeType()
        {
            return UnderlyingNodeType;
        }
    }
}
