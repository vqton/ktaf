using AMS.Domain.Entities.Assets;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface cho việc quản lý tài sản cố định.
/// </summary>
public interface IFixedAssetRepository
{
    /// <summary>
    /// Lấy tài sản theo ID.
    /// </summary>
    /// <param name="id">ID tài sản.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    /// <returns>Tài sản nếu tìm thấy, null nếu không.</returns>
    Task<FixedAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tài sản theo mã tài sản.
    /// </summary>
    /// <param name="assetCode">Mã tài sản.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    /// <returns>Tài sản nếu tìm thấy, null nếu không.</returns>
    Task<FixedAsset?> GetByCodeAsync(string assetCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách tài sản có phân trang.
    /// </summary>
    /// <param name="page">Số trang (1-based).</param>
    /// <param name="pageSize">Số phần tử mỗi trang.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    /// <returns>Tuple chứa danh sách tài sản và tổng số bản ghi.</returns>
    Task<(IEnumerable<FixedAsset> Assets, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách tài sản đang hoạt động.
    /// </summary>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    /// <returns>Danh sách tài sản đang hoạt động.</returns>
    Task<IEnumerable<FixedAsset>> GetActiveAssetsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới tài sản.
    /// </summary>
    /// <param name="asset">Tài sản cần thêm.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    Task AddAsync(FixedAsset asset, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật tài sản.
    /// </summary>
    /// <param name="asset">Tài sản cần cập nhật.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    Task UpdateAsync(FixedAsset asset, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa tài sản (soft delete).
    /// </summary>
    /// <param name="id">ID tài sản cần xóa.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới lịch trích khấu hao.
    /// </summary>
    /// <param name="schedule">Lịch trích khấu hao.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    Task AddDepreciationScheduleAsync(DepreciationSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy lịch trích khấu hao theo kỳ.
    /// </summary>
    /// <param name="assetId">ID tài sản.</param>
    /// <param name="year">Năm.</param>
    /// <param name="month">Tháng.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    /// <returns>Lịch trích khấu hao nếu tìm thấy, null nếu không.</returns>
    Task<DepreciationSchedule?> GetDepreciationScheduleAsync(Guid assetId, int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách lịch trích khấu hao của tài sản.
    /// </summary>
    /// <param name="assetId">ID tài sản.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    /// <returns>Danh sách lịch trích khấu hao.</returns>
    Task<IEnumerable<DepreciationSchedule>> GetDepreciationSchedulesAsync(Guid assetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật lịch trích khấu hao.
    /// </summary>
    /// <param name="schedule">Lịch trích khấu hao.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    Task UpdateDepreciationScheduleAsync(DepreciationSchedule schedule, CancellationToken cancellationToken = default);
}