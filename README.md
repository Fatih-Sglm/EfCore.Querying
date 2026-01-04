# NextResta.Filtering Library Documentation

## Overview

`NextResta.Filtering` is a dynamic query filtering library for Entity Framework Core, compatible with Kendo UI / Telerik Grid. It provides:

- **Dynamic Filtering** - Build LINQ expressions from filter descriptors
- **Dynamic Sorting** - Apply sorting with multiple fields
- **Pagination** - Skip/Take with total count
- **Keyset Pagination** - Cursor-based pagination for large datasets
- **Simplified POCO Model** - Easy to use with JSON serialization or manual object creation
- **Custom Type Handlers** - Extensible for special types (JSONB, MultiLanguage, etc.)
- **Filter Transformations** - Map field names and transform values
- **Collection Filtering** - Any() expressions for one-to-many and many-to-many

---

## Project Structure

```
NextResta.Filtering/
├── Abstractions/
│   ├── IFilterTransformable.cs      # Interface for entity filter transformations
│   ├── ISortTransformable.cs        # Interface for entity sort transformations
│   ├── IQueryTransformable.cs       # Combined interface
│   └── ITypeExpressionBuilder.cs    # Interface for custom type expression builders
├── Configuration/
│   └── QueryFilterConfiguration.cs     # Static configuration and options
├── Extensions/
│   ├── FilterExtensions.cs             # Core filtering logic (ApplyFilters)
│   ├── SortingExtensions.cs            # Sorting logic (ApplySorting)
│   ├── QueryFilterExtensions.cs        # Main entry point (ApplyQueryFilterAsync)
│   ├── FilterOperatorExtensions.cs     # Operator token conversion
│   └── ServiceCollectionExtensions.cs  # DI registration
├── Infrastructure/
│   └── Binders/
│       └── JsonObjectModelBinder.cs    # ASP.NET model binder for dynamic values
└── Models/
    ├── Constants/
    │   └── FilterConstants.cs          # Operator tokens, logic strings
    ├── Enums/
    │   ├── FilterOperator.cs           # Filter operator enum
    │   └── ListSortDirection.cs        # Sort direction enum
    ├── Filters/
    │   ├── FilterDescriptorBase.cs     # Base class with case sensitivity
    │   └── FilterCondition.cs          # Unified filter condition (Individual or Group)
    ├── Requests/
    │   └── QueryFilterRequest.cs       # Main request model
    ├── Responses/
    │   └── ListViewResponse.cs         # Paginated response model
    └── Sorting/
        └── SortDescriptor.cs           # Sort field descriptor
```

---

## Core Models

### QueryFilterRequest
```csharp
public class QueryFilterRequest
{
    public int Take { get; set; } = 20;
    public int Skip { get; set; } = 0;
    public List<SortDescriptor>? Sort { get; set; }
    public FilterCondition? Filter { get; set; } // Points to the root FilterCondition
    public bool IncludeCount { get; set; } = true;
    public FilterCondition? Cursor { get; set; } // Keyset pagination
}
```

### FilterCondition (Unified Model)
The `FilterCondition` class can represent either a single condition or a nested group of conditions.

```csharp
public class FilterCondition : FilterDescriptorBase
{
    // For single condition
    public string? Field { get; set; }
    public FilterOperator Operator { get; set; } = FilterOperator.IsEqualTo;
    public object? Value { get; set; }

    // For nested groups
    public string? Logic { get; set; } // "and" or "or"
    public List<FilterCondition>? Filters { get; set; }
}
```

---

## Usage Examples

### Basic Usage
```csharp
// In Controller
[HttpGet]
public async Task<ActionResult> GetProducts([FromQuery] QueryFilterRequest request)
{
    var query = _context.Products.Where(p => !p.IsDeleted);
    var result = await query.ApplyQueryFilterAsync(request);
    return Ok(result);
}
```

### Manual Request Creation
```csharp
var request = new QueryFilterRequest
{
    Take = 10,
    Filter = new FilterCondition 
    {
        Logic = "and",
        Filters = new List<FilterCondition>
        {
            new FilterCondition("IsActive", FilterOperator.IsEqualTo, true),
            new FilterCondition("Price", FilterOperator.IsGreaterThan, 500)
        }
    }
};

var result = await query.ApplyQueryFilterAsync(request);
```

### Custom Specialized Filters
You can inherit from `FilterCondition` to add type-safe properties for custom builders.

```csharp
public class MultiLangFilter : FilterCondition
{
    public string LanguageCode { get; set; } = "tr";
}

// Usage
var request = new QueryFilterRequest
{
    Filter = new MultiLangFilter 
    { 
        Field = "Name", 
        Value = "SearchTerm", 
        LanguageCode = "en" 
    }
};
```

---

## Custom Type Expression Builders

For special types like JSONB, MultiLanguageContent, etc.:

### ITypeExpressionBuilder Interface
```csharp
public interface ITypeExpressionBuilder
{
    Expression? BuildExpression(
        Expression propertyAccess, 
        FilterCondition condition,
        QueryFilterOptions options);
}
```

### MultiLanguageContent Example
```csharp
public class MultiLanguageExpressionBuilder : ITypeExpressionBuilder<MultiLanguageContent>
{
    public Expression? BuildExpression(Expression propertyAccess, FilterCondition condition, QueryFilterOptions options)
    {
        // Check if condition is specialized
        string? languageCode = condition is MultiLangFilter mlf ? mlf.LanguageCode : "tr";
        
        var translationsProperty = Expression.Property(propertyAccess, "Content");
        
        // Build custom LINQ expression...
        return BuildContainsExpression(translationsProperty, languageCode, condition.Value);
    }
}
```

---

## Configuration

### Registration via DI
```csharp
services.AddQueryFilter(options =>
{
    options.MaxPageSize = 100;
    options.RegisterTypeBuilder(new MultiLanguageExpressionBuilder());
});
```

---

## HTTP Query String & JSON Format

The library works perfectly with standard JSON or Query String parameters mapping to `QueryFilterRequest`.

### JSON Body (POST)
```json
{
  "take": 20,
  "skip": 0,
  "filter": {
    "logic": "and",
    "filters": [
      { "field": "name", "operator": "Contains", "value": "VIP" },
      { "field": "isActive", "operator": "IsEqualTo", "value": true }
    ]
  }
}
```

---

## Extension Methods

### ApplyQueryFilterAsync (Main Entry Point)
Main method used to apply filtering, sorting, and pagination to an `IQueryable`.

```csharp
Task<ListViewResponse<T>> ApplyQueryFilterAsync<T>(
    this IQueryable<T> query,
    QueryFilterRequest request,
    QueryFilterOptions? options = null);
```
