namespace DedsiNative.WorkItemListener;

internal sealed record WorkItemCandidate(int Id, int Revision, string Title, string State, string Tags);
