// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

namespace RegistryApi.DTOs;

// Using record struct for simple immutable data containers
public record struct PaginationMetadata(
    int TotalCount,
    int PageSize,
    int CurrentPage,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);

//// Base class for pagination query parameters remains largely the same
//public class PaginationParams
//{
//    private const int MaxPageSize = 50;
//    private int _pageSize = 10;

//    public int PageNumber { get; set; } = 1;

//    public int PageSize
//    {
//        get => _pageSize;
//        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : Math.Max(1, value);
//    }
//}
