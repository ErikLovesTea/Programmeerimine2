using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Infrastructure.Paging;
using Microsoft.EntityFrameworkCore;
namespace KooliProjekt.Application.Data;
public static class PagingExtensions
{
    // Õpetaja näite põhjal: lisatud tühistamine ja piirväärtuste normaliseerimine.
    public static async Task<PagedResult<T>> GetPagedAsync<T>(this IQueryable<T> query,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        var result = new PagedResult<T>
        {
            CurrentPage = page, PageSize = pageSize,
            RowCount = await query.CountAsync(cancellationToken)
        };
        result.PageCount = (int)Math.Ceiling((double)result.RowCount / pageSize);
        var skip = ((long)page - 1) * pageSize;
        if (skip < result.RowCount)
            result.Results = await query.Skip((int)skip).Take(pageSize).ToListAsync(cancellationToken);
        return result;
    }
}
