package com.tonvq.accountingerp.infrastructure.persistence.entity;

import lombok.*;
import javax.persistence.*;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;

/**
 * JPA Entity - Hợp đồng dịch vụ (Service Contract)
 * VAS 14/15: Công nhận doanh thu theo từng milestone hoặc % hoàn thành
 * Lifecycle: DRAFT → ACTIVE → IN_PROGRESS → COMPLETED
 */
@Entity
@Table(name = "hop_dong_dich_vu", schema = "public", indexes = {
    @Index(name = "idx_hopdong_ma", columnList = "ma_hop_dong", unique = true),
    @Index(name = "idx_hopdong_khach_hang", columnList = "khach_hang_id"),
    @Index(name = "idx_hopdong_trang_thai", columnList = "trang_thai")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class HopDongDichVuEntity {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "ma_hop_dong", nullable = false, unique = true, length = 50)
    private String maHopDong;
    
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "khach_hang_id", nullable = false)
    private KhachHangEntity khachHang;
    
    @Column(name = "ten_hop_dong", nullable = false, length = 500)
    private String tenHopDong;
    
    @Column(name = "ngay_bat_dau", nullable = false)
    private LocalDate ngayBatDau;
    
    @Column(name = "ngay_ket_thuc", nullable = false)
    private LocalDate ngayKetThuc;
    
    @Column(name = "gia_hop_dong", nullable = false, precision = 18, scale = 2)
    private BigDecimal giaHopDong;
    
    @Column(name = "so_milestone", nullable = false)
    private Integer soMilestone; // Số mốc thanh toán
    
    @Column(name = "milestone_hoan_thanh", nullable = false)
    @Builder.Default
    private Integer milestoneHoanThanh = 0; // Số mốc đã hoàn thành
    
    @Column(name = "percent_complete", nullable = false, precision = 5, scale = 2)
    @Builder.Default
    private BigDecimal percentComplete = BigDecimal.ZERO; // % hoàn thành (0-100)
    
    @Column(name = "doanh_thu_cong_nhan", nullable = false, precision = 18, scale = 2)
    @Builder.Default
    private BigDecimal doanhThuCongNhan = BigDecimal.ZERO; // Doanh thu công nhận theo VAS 14/15
    
    @Column(name = "phuong_phap_cong_nhan", nullable = false, length = 20)
    private String phuongPhapCongNhan; // MILESTONE hoặc PERCENT_COMPLETION
    
    @Column(name = "trang_thai", nullable = false, length = 20)
    @Builder.Default
    private String trangThai = "DRAFT"; // DRAFT, ACTIVE, IN_PROGRESS, COMPLETED
    
    @Column(name = "created_by", nullable = false)
    private Long createdBy;
    
    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;
    
    @Column(name = "activated_by")
    private Long activatedBy;
    
    @Column(name = "activated_at")
    private LocalDateTime activatedAt;
    
    @Column(name = "completed_by")
    private Long completedBy;
    
    @Column(name = "completed_at")
    private LocalDateTime completedAt;
    
    @Column(name = "updated_at")
    private LocalDateTime updatedAt;
    
    @Column(name = "updated_by")
    private Long updatedBy;
    
    @Column(name = "is_deleted", nullable = false)
    @Builder.Default
    private Boolean isDeleted = false;
    
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
