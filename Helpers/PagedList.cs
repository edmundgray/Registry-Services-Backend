// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;

namespace RegistryApi.Helpers;

/// <summary>
/// Represents a paginated list of items.
/// Uses primary constructor and collection expression.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PagedList<T>(List<T> items, int count, int pageNumber, int pageSize)
{
    public List<T> Items { get; } = items;
    public int PageNumber { get; } = pageNumber;
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = count;
    public int TotalPages { get; } = (int)Math.Ceiling(count / (double)pageSize);

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates a paginated list asynchronously from a queryable source.
    /// </summary>
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        // Ensure pageNumber and pageSize are valid
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
