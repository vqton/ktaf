package com.tonvq.accountingerp.infrastructure.persistence.entity;

import lombok.*;
import javax.persistence.*;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;

/**
 * JPA Entity - Hóa đơn (Invoice)
 * Lifecycle: DRAFT → ISSUED → CANCELLED
 */
@Entity
@Table(name = "hoa_don", schema = "public", indexes = {
    @Index(name = "idx_hoa_don_ma", columnList = "ma_hoa_don", unique = true),
    @Index(name = "idx_hoa_don_don_hang", columnList = "don_hang_id"),
    @Index(name = "idx_hoa_don_trang_thai", columnList = "trang_thai")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class HoaDonEntity {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "ma_hoa_don", nullable = false, unique = true, length = 50)
    private String maHoaDon;
    
    @OneToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "don_hang_id", nullable = false, unique = true)
    private DonHangEntity donHang;
    
    @Column(name = "ngay_hoa_don", nullable = false)
    private LocalDate ngayHoaDon;
    
    @Column(name = "tong_tien_thanh_toan", nullable = false, precision = 18, scale = 2)
    private BigDecimal tongTienThanhToan;
    
    @Column(name = "tien_con_no", nullable = false, precision = 18, scale = 2)
    private BigDecimal tienConNo;
    
    @Column(name = "trang_thai", nullable = false, length = 20)
    @Builder.Default
    private String trangThai = "DRAFT"; // DRAFT, ISSUED, CANCELLED
    
    @Column(name = "issued_at")
    private LocalDateTime issuedAt;
    
    @Column(name = "issued_by")
    private Long issuedBy;
    
    @Column(name = "cancelled_at")
    private LocalDateTime cancelledAt;
    
    @Column(name = "cancelled_by")
    private Long cancelledBy;
    
    @Column(name = "cancel_reason", length = 500)
    private String cancelReason;
    
    @Column(name = "created_by", nullable = false)
    private Long createdBy;
    
    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;
    
    @Column(name = "updated_at")
    private LocalDateTime updatedAt;
    
    @Column(name = "updated_by")
    private Long updatedBy;
    
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
