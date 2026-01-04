using EfCore.Querying.Infrastructure.Converters;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace EfCore.Querying;

/// <summary>
/// Sort direction for query sorting
/// </summary>
[JsonConverter(typeof(ListSortDirectionJsonConverter))]
[ModelBinder(typeof(ListSortDirectionModelBinder))]
public enum ListSortDirection
{
    /// <summary>Ascending sort</summary>
    Ascending,
    /// <summary>Descending sort</summary>
    Descending
}
