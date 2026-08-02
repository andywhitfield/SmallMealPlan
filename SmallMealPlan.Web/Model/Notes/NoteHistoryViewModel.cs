using SmallMealPlan.Model;

namespace SmallMealPlan.Web.Model.Notes;

public class NoteHistoryViewModel(NoteHistory noteHistory)
{
    public int NoteHistoryId => noteHistory.NoteHistoryId;

    public string TitleForDisplay => noteHistory.Title ?? NoteViewModel.Title(noteHistory.NoteText);

    public bool ShowExpandIcon => !string.IsNullOrEmpty(noteHistory.Title) || TitleForDisplay != noteHistory.NoteText;

    public string AuditDateTime => noteHistory.AuditDateTime.ToString("yyyy-MM-dd HH:mm:ss");
}
