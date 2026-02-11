package com.tonvq.accountingerp.infrastructure.persistence.repository;

import com.tonvq.accountingerp.infrastructure.persistence.entity.HoaDonEntity;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import java.util.List;
import java.util.Optional;

/**
 * Spring Data JPA Repository - HoaDon (Invoice)
 */
public interface JpaHoaDonRepository extends BaseRepository<HoaDonEntity, Long> {
    Optional<HoaDonEntity> findByMaHoaDon(String maHoaDon);
    
    @Query("SELECT hd FROM HoaDonEntity hd WHERE hd.trangThai = :trangThai AND hd.isDeleted = false")
    List<HoaDonEntity> findByTrangThai(@Param("trangThai") String trangThai);
}
