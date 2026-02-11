package com.tonvq.accountingerp.infrastructure.persistence.repository;

import com.tonvq.accountingerp.infrastructure.persistence.entity.TonKhoEntity;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import java.util.List;
import java.util.Optional;

/**
 * Spring Data JPA Repository - TonKho (Inventory)
 */
public interface JpaTonKhoRepository extends BaseRepository<TonKhoEntity, Long> {
    Optional<TonKhoEntity> findByMaSanPham(String maSanPham);
    
    @Query("SELECT tk FROM TonKhoEntity tk WHERE tk.maKho = :maKho AND tk.isDeleted = false")
    List<TonKhoEntity> findByKho(@Param("maKho") String maKho);
    
    @Query("SELECT tk FROM TonKhoEntity tk WHERE tk.soLuongCuoi <= 0 AND tk.isDeleted = false")
    List<TonKhoEntity> findOutOfStockProducts();
}
