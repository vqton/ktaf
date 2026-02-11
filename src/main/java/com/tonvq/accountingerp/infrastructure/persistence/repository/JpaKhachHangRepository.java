package com.tonvq.accountingerp.infrastructure.persistence.repository;

import com.tonvq.accountingerp.infrastructure.persistence.entity.KhachHangEntity;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import java.util.List;
import java.util.Optional;

/**
 * Spring Data JPA Repository - KhachHang (Customer)
 */
public interface JpaKhachHangRepository extends BaseRepository<KhachHangEntity, Long> {
    Optional<KhachHangEntity> findByMaKhachHang(String maKhachHang);
    
    @Query("SELECT kh FROM KhachHangEntity kh WHERE kh.email = :email AND kh.isDeleted = false")
    Optional<KhachHangEntity> findByEmail(@Param("email") String email);
    
    @Query("SELECT kh FROM KhachHangEntity kh WHERE kh.dienThoai = :dienThoai AND kh.isDeleted = false")
    Optional<KhachHangEntity> findByDienThoai(@Param("dienThoai") String dienThoai);
}
