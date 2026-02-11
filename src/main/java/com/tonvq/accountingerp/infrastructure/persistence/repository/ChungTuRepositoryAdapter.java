package com.tonvq.accountingerp.infrastructure.persistence.repository;

import com.tonvq.accountingerp.infrastructure.persistence.entity.ChungTuEntity;
import com.tonvq.accountingerp.domain.repository.ChungTuRepository;
import com.tonvq.accountingerp.domain.model.entity.ChungTu;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;
import java.time.LocalDate;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

/**
 * Repository Adapter - Adapter giữa domain layer và JPA infrastructure
 * Chuyển đổi giữa ChungTuEntity (JPA) và ChungTu (Domain)
 */
@Repository
@RequiredArgsConstructor
public class ChungTuRepositoryAdapter implements ChungTuRepository {
    
    private final JpaChungTuRepository jpaRepository;
    
    @Override
    public ChungTu save(ChungTu domain) {
        ChungTuEntity entity = toDomainModel(domain);
        ChungTuEntity saved = jpaRepository.save(entity);
        return toEntity(saved);
    }
    
    @Override
    public Optional<ChungTu> findById(Long id) {
        return jpaRepository.findById(id).map(this::toEntity);
    }
    
    @Override
    public Optional<ChungTu> findByMaChungTu(String maChungTu) {
        return jpaRepository.findByMaChungTu(maChungTu).map(this::toEntity);
    }
    
    @Override
    public List<ChungTu> findAll() {
        return jpaRepository.findAllByIsDeletedFalse().stream()
            .map(this::toEntity)
            .collect(Collectors.toList());
    }
    
    @Override
    public List<ChungTu> findByTrangThai(String trangThai) {
        return jpaRepository.findByTrangThai(trangThai).stream()
            .map(this::toEntity)
            .collect(Collectors.toList());
    }
    
    @Override
    public List<ChungTu> findByLoaiChungTu(String loaiChungTu) {
        return jpaRepository.findByLoaiChungTu(loaiChungTu).stream()
            .map(this::toEntity)
            .collect(Collectors.toList());
    }
    
    @Override
    public List<ChungTu> findByDateRange(LocalDate startDate, LocalDate endDate) {
        return jpaRepository.findByDateRange(startDate, endDate).stream()
            .map(this::toEntity)
            .collect(Collectors.toList());
    }
    
    @Override
    public void delete(Long id) {
        jpaRepository.deleteById(id);
    }
    
    @Override
    public boolean existsById(Long id) {
        return jpaRepository.existsById(id);
    }
    
    // Helper methods to convert between JPA entity and domain model
    private ChungTu toEntity(ChungTuEntity jpaEntity) {
        if (jpaEntity == null) return null;
        
        return ChungTu.builder()
            .id(jpaEntity.getId())
            .maChungTu(jpaEntity.getMaChungTu())
            .loaiChungTu(jpaEntity.getLoaiChungTu())
            .ngayChungTu(jpaEntity.getNgayChungTu())
            .ndChungTu(jpaEntity.getNdChungTu())
            .soTien(jpaEntity.getSoTien())
            .trangThai(jpaEntity.getTrangThai())
            .createdBy(jpaEntity.getCreatedBy())
            .createdAt(jpaEntity.getCreatedAt())
            .approvedBy(jpaEntity.getApprovedBy())
            .approvedAt(jpaEntity.getApprovedAt())
            .postedBy(jpaEntity.getPostedBy())
            .postedAt(jpaEntity.getPostedAt())
            .build();
    }
    
    private ChungTuEntity toDomainModel(ChungTu domain) {
        if (domain == null) return null;
        
        return ChungTuEntity.builder()
            .id(domain.getId())
            .maChungTu(domain.getMaChungTu())
            .loaiChungTu(domain.getLoaiChungTu())
            .ngayChungTu(domain.getNgayChungTu())
            .ndChungTu(domain.getNdChungTu())
            .soTien(domain.getSoTien())
            .trangThai(domain.getTrangThai())
            .createdBy(domain.getCreatedBy())
            .createdAt(domain.getCreatedAt())
            .approvedBy(domain.getApprovedBy())
            .approvedAt(domain.getApprovedAt())
            .postedBy(domain.getPostedBy())
            .postedAt(domain.getPostedAt())
            .isDeleted(false)
            .build();
    }
}
