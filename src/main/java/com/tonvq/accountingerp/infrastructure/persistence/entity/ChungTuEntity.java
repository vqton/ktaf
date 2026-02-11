package com.tonvq.accountingerp.infrastructure.persistence.entity;

import lombok.*;
import javax.persistence.*;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.*;

/**
 * JPA Entity - Chứng từ (Document/Voucher)
 * Lifecycle: DRAFT → APPROVED → POSTED → LOCKED → CANCELLED
 * Tuân thủ TT 99/2025/TT-BTC Phụ lục I
 */
@Entity
@Table(name = "chung_tu", schema = "public", indexes = {
    @Index(name = "idx_chung_tu_ma", columnList = "ma_chung_tu", unique = true),
    @Index(name = "idx_chung_tu_trang_thai", columnList = "trang_thai"),
    @Index(name = "idx_chung_tu_ngay", columnList = "ngay_chung_tu")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class ChungTuEntity {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id")
    private Long id;
    
    @Column(name = "ma_chung_tu", nullable = false, unique = true, length = 50)
    private String maChungTu;
    
    @Column(name = "loai_chung_tu", nullable = false, length = 20)
    private String loaiChungTu; // HDDON, HOADON, PHIEUNH, PHIEUXUAT, etc.
    
    @Column(name = "ngay_chung_tu", nullable = false)
    private LocalDate ngayChungTu;
    
    @Column(name = "nd_chung_tu", length = 500)
    private String ndChungTu; // Nội dung
    
    @Column(name = "so_tien", nullable = false, precision = 18, scale = 2)
    private BigDecimal soTien;
    
    @Column(name = "trang_thai", nullable = false, length = 20)
    private String trangThai; // DRAFT, APPROVED, POSTED, LOCKED, CANCELLED
    
    @Column(name = "created_by", nullable = false)
    private Long createdBy;
    
    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;
    
    @Column(name = "approved_by")
    private Long approvedBy;
    
    @Column(name = "approved_at")
    private LocalDateTime approvedAt;
    
    @Column(name = "posted_by")
    private Long postedBy;
    
    @Column(name = "posted_at")
    private LocalDateTime postedAt;
    
    @Column(name = "locked_by")
    private Long lockedBy;
    
    @Column(name = "locked_at")
    private LocalDateTime lockedAt;
    
    @Column(name = "cancelled_by")
    private Long cancelledBy;
    
    @Column(name = "cancelled_at")
    private LocalDateTime cancelledAt;
    
    @Column(name = "cancel_reason", length = 500)
    private String cancelReason;
    
    @Column(name = "approval_reason", length = 500)
    private String approvalReason;
    
    @Column(name = "updated_at")
    private LocalDateTime updatedAt;
    
    @Column(name = "updated_by")
    private Long updatedBy;
    
    @Column(name = "is_deleted", nullable = false)
    @Builder.Default
    private Boolean isDeleted = false;
    
    // Relationships
    @OneToMany(mappedBy = "chungTu", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.LAZY)
    private List<ButToanEntity> butToans = new ArrayList<>();
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "tao_boi_user_id")
    private UserEntity createdByUser;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "duyet_boi_user_id")
    private UserEntity approvedByUser;
    
    @PrePersist
    protected void onCreate() {
        createdAt = LocalDateTime.now();
        updatedAt = LocalDateTime.now();
        if (trangThai == null) {
            trangThai = "DRAFT";
        }
    }
    
    @PreUpdate
    protected void onUpdate() {
        updatedAt = LocalDateTime.now();
    }
}
