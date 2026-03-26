using AMS.Domain.Entities.Assets;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho việc quản lý tài sản cố định.
/// </summary>
public class FixedAssetRepository : IFixedAssetRepository
{
    private readonly AMSDbContext _context;

    public FixedAssetRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<FixedAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FixedAssets
            .AsNoTracking()
            .Include(a => a.AssetGroup)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FixedAsset?> GetByCodeAsync(string assetCode, CancellationToken cancellationToken = default)
    {
        return await _context.FixedAssets
            .AsNoTracking()
            .Include(a => a.AssetGroup)
            .FirstOrDefaultAsync(a => a.AssetCode == assetCode, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<FixedAsset> Assets, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.FixedAssets
            .AsNoTracking()
            .Include(a => a.AssetGroup)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var assets = await query
            .OrderBy(a => a.AssetCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (assets, totalCount);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FixedAsset>> GetActiveAssetsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FixedAssets
            .AsNoTracking()
            .Where(a => a.Status == Domain.Enums.AssetStatus.Active)
            .Include(a => a.AssetGroup)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FixedAsset asset, CancellationToken cancellationToken = default)
    {
        await _context.FixedAssets.AddAsync(asset, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(FixedAsset asset, CancellationToken cancellationToken = default)
    {
        _context.FixedAssets.Update(asset);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _context.FixedAssets.FindAsync(new object[] { id }, cancellationToken);
        if (asset != null)
        {
            asset.IsDeleted = true;
            asset.ModifiedAt = DateTime.UtcNow;
        }
    }

    /// <inheritdoc />
    public async Task AddDepreciationScheduleAsync(DepreciationSchedule schedule, CancellationToken cancellationToken = default)
    {
        await _context.DepreciationSchedules.AddAsync(schedule, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DepreciationSchedule?> GetDepreciationScheduleAsync(Guid assetId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.DepreciationSchedules
            .FirstOrDefaultAsync(s => s.AssetId == assetId && s.PeriodYear == year && s.PeriodMonth == month, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DepreciationSchedule>> GetDepreciationSchedulesAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        return await _context.DepreciationSchedules
            .Where(s => s.AssetId == assetId)
            .OrderBy(s => s.PeriodYear)
            .ThenBy(s => s.PeriodMonth)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateDepreciationScheduleAsync(DepreciationSchedule schedule, CancellationToken cancellationToken = default)
    {
        _context.DepreciationSchedules.Update(schedule);
        await Task.CompletedTask;
    }
}