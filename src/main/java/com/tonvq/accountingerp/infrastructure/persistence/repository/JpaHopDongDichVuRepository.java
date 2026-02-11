package com.tonvq.accountingerp.infrastructure.persistence.repository;

import com.tonvq.accountingerp.infrastructure.persistence.entity.HopDongDichVuEntity;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import java.util.List;
import java.util.Optional;

/**
 * Spring Data JPA Repository - HopDongDichVu (Service Contract)
 */
public interface JpaHopDongDichVuRepository extends BaseRepository<HopDongDichVuEntity, Long> {
    Optional<HopDongDichVuEntity> findByMaHopDong(String maHopDong);
    
    @Query("SELECT hd FROM HopDongDichVuEntity hd WHERE hd.trangThai = :trangThai AND hd.isDeleted = false")
    List<HopDongDichVuEntity> findByTrangThai(@Param("trangThai") String trangThai);
    
    @Query("SELECT hd FROM HopDongDichVuEntity hd WHERE hd.khachHang.id = :khachHangId AND hd.isDeleted = false")
    List<HopDongDichVuEntity> findByKhachHang(@Param("khachHangId") Long khachHangId);
}
