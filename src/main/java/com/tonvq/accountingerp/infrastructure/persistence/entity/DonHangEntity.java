package com.tonvq.accountingerp.infrastructure.persistence.entity;

import lombok.*;
import javax.persistence.*;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.*;

/**
 * JPA Entity - Đơn hàng (Sales Order)
 * Lifecycle: DRAFT → CONFIRMED → SHIPPING → DELIVERED → PAID
 * TT 99/2025 tuân thủ tính toán VAT, theo dõi thanh toán
 */
@Entity
@Table(name = "don_hang", schema = "public", indexes = {
    @Index(name = "idx_don_hang_ma", columnList = "ma_don_hang", unique = true),
    @Index(name = "idx_don_hang_trang_thai", columnList = "trang_thai"),
    @Index(name = "idx_don_hang_khach_hang", columnList = "khach_hang_id"),
    @Index(name = "idx_don_hang_ngay", columnList = "ngay_don_hang")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class DonHangEntity {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "ma_don_hang", nullable = false, unique = true, length = 50)
    private String maDonHang;
    
    @Column(name = "loai_don_hang", nullable = false, length = 20)
    private String loaiDonHang; // BAN, MUA
    
    @Column(name = "ngay_don_hang", nullable = false)
    private LocalDate ngayDonHang;
    
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "khach_hang_id", nullable = false)
    private KhachHangEntity khachHang;
    
    @Column(name = "tong_tien", nullable = false, precision = 18, scale = 2)
    private BigDecimal tongTien;
    
    @Column(name = "tien_da_thanh_toan", nullable = false, precision = 18, scale = 2)
    @Builder.Default
    private BigDecimal tienDaThanhToan = BigDecimal.ZERO;
    
    @Column(name = "tien_con_no", nullable = false, precision = 18, scale = 2)
    private BigDecimal tienConNo;
    
    @Column(name = "ty_le_vat", precision = 5, scale = 2)
    private BigDecimal tyLeVAT; // % VAT (e.g., 10.00)
    
    @Column(name = "tien_vat", precision = 18, scale = 2)
    private BigDecimal tienVAT;
    
    @Column(name = "trang_thai", nullable = false, length = 20)
    @Builder.Default
    private String trangThai = "DRAFT"; // DRAFT, CONFIRMED, SHIPPING, DELIVERED, PAID, CANCELLED
    
    @Column(name = "ngay_giao_hang")
    private LocalDate ngayGiaoHang;
    
    @Column(name = "dia_chi_giao", length = 500)
    private String diaChiGiao;
    
    @Column(name = "phuong_thuc_thanh_toan", length = 50)
    private String phuongThucThanhToan; // TIEN_MAT, CHUYEN_KHOAN, CHI_TRAI, etc.
    
    @Column(name = "created_by", nullable = false)
    private Long createdBy;
    
    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;
    
    @Column(name = "confirmed_by")
    private Long confirmedBy;
    
    @Column(name = "confirmed_at")
    private LocalDateTime confirmedAt;
    
    @Column(name = "shipped_by")
    private Long shippedBy;
    
    @Column(name = "shipped_at")
    private LocalDateTime shippedAt;
    
    @Column(name = "updated_at")
    private LocalDateTime updatedAt;
    
    @Column(name = "updated_by")
    private Long updatedBy;
    
    @Column(name = "is_deleted", nullable = false)
    @Builder.Default
    private Boolean isDeleted = false;
    
    @OneToMany(mappedBy = "donHang", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.LAZY)
    private List<DonHangChiTietEntity> chiTiets = new ArrayList<>();
    
    @OneToOne(mappedBy = "donHang", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    private HoaDonEntity hoaDon;
    
    @PrePersist
    protected void onCreate() {
        createdAt = LocalDateTime.now();
        updatedAt = LocalDateTime.now();
        if (trangThai == null) {
            trangThai = "DRAFT";
        }
        if (tienConNo == null) {
            tienConNo = tongTien;
        }
    }
    
    @PreUpdate
    protected void onUpdate() {
        updatedAt = LocalDateTime.now();
    }
}
