package com.tonvq.accountingerp.infrastructure.persistence.repository;

import com.tonvq.accountingerp.infrastructure.persistence.entity.ChungTuEntity;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import java.time.LocalDate;
import java.util.List;
import java.util.Optional;

/**
 * Spring Data JPA Repository - ChungTu (Document)
 * Custom queries for document lifecycle queries
 */
public interface JpaChungTuRepository extends BaseRepository<ChungTuEntity, Long> {
    
    Optional<ChungTuEntity> findByMaChungTu(String maChungTu);
    
    @Query("SELECT ct FROM ChungTuEntity ct WHERE ct.trangThai = :trangThai AND ct.isDeleted = false")
    List<ChungTuEntity> findByTrangThai(@Param("trangThai") String trangThai);
    
    @Query("SELECT ct FROM ChungTuEntity ct WHERE ct.loaiChungTu = :loaiChungTu AND ct.isDeleted = false")
    List<ChungTuEntity> findByLoaiChungTu(@Param("loaiChungTu") String loaiChungTu);
    
    @Query("SELECT ct FROM ChungTuEntity ct WHERE ct.ngayChungTu BETWEEN :startDate AND :endDate AND ct.isDeleted = false ORDER BY ct.ngayChungTu")
    List<ChungTuEntity> findByDateRange(@Param("startDate") LocalDate startDate, @Param("endDate") LocalDate endDate);
    
    @Query("SELECT ct FROM ChungTuEntity ct WHERE ct.createdBy = :userId AND ct.isDeleted = false")
    List<ChungTuEntity> findByCreatedBy(@Param("userId") Long userId);
}
