using System.Text.Json.Serialization;

namespace EfCore.Querying;

/// <summary>
/// Sort descriptor for defining field and direction
/// </summary>
public class SortDescriptor
{
    /// <summary>
    /// Field name to sort by
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Alias for Field (Kendo/Telerik compatibility)
    /// </summary>
    public string Member
    {
        get => Field;
        set => Field = value;
    }

    /// <summary>
    /// Sort direction
    /// </summary>
    public ListSortDirection Dir { get; set; } = ListSortDirection.Ascending;

    /// <summary>
    /// Default constructor
    /// </summary>
    public SortDescriptor() { }

    /// <summary>
    /// Create a sort descriptor with field and direction
    /// </summary>
    public SortDescriptor(string field, ListSortDirection direction)
    {
        Field = field;
        Dir = direction;
    }
}
