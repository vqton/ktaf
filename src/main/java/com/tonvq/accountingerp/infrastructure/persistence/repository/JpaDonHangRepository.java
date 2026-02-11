package com.tonvq.accountingerp.infrastructure.persistence.repository;

import com.tonvq.accountingerp.infrastructure.persistence.entity.DonHangEntity;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import java.time.LocalDate;
import java.util.List;
import java.util.Optional;

/**
 * Spring Data JPA Repository - DonHang (Sales Order)
 */
public interface JpaDonHangRepository extends BaseRepository<DonHangEntity, Long> {
    
    Optional<DonHangEntity> findByMaDonHang(String maDonHang);
    
    @Query("SELECT dh FROM DonHangEntity dh WHERE dh.trangThai = :trangThai AND dh.isDeleted = false")
    List<DonHangEntity> findByTrangThai(@Param("trangThai") String trangThai);
    
    @Query("SELECT dh FROM DonHangEntity dh WHERE dh.khachHang.id = :khachHangId AND dh.isDeleted = false")
    List<DonHangEntity> findByKhachHang(@Param("khachHangId") Long khachHangId);
    
    @Query("SELECT dh FROM DonHangEntity dh WHERE dh.tienConNo > 0 AND dh.isDeleted = false ORDER BY dh.ngayDonHang DESC")
    List<DonHangEntity> findUnpaidOrders();
    
    @Query("SELECT dh FROM DonHangEntity dh WHERE dh.ngayDonHang BETWEEN :startDate AND :endDate AND dh.isDeleted = false")
    List<DonHangEntity> findByDateRange(@Param("startDate") LocalDate startDate, @Param("endDate") LocalDate endDate);
}
