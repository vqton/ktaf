package com.tonvq.accountingerp.infrastructure.persistence.entity;

import lombok.*;
import javax.persistence.*;
import java.math.BigDecimal;
import java.time.LocalDateTime;

/**
 * JPA Entity - Tồn kho (Inventory)
 * TT 99/2025 Phụ lục II: Hỗ trợ FIFO, LIFO, Trung bình
 */
@Entity
@Table(name = "ton_kho", schema = "public", indexes = {
    @Index(name = "idx_ton_kho_ma_san_pham", columnList = "ma_san_pham", unique = true),
    @Index(name = "idx_ton_kho_kho", columnList = "ma_kho")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class TonKhoEntity {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "ma_san_pham", nullable = false, unique = true, length = 50)
    private String maSanPham;
    
    @Column(name = "ten_san_pham", nullable = false, length = 500)
    private String tenSanPham;
    
    @Column(name = "ma_kho", nullable = false, length = 50)
    private String maKho;
    
    @Column(name = "so_luong_dau", nullable = false, precision = 18, scale = 4)
    private BigDecimal soLuongDau; // Số lượng đầu kỳ
    
    @Column(name = "so_luong_nhap", nullable = false, precision = 18, scale = 4)
    @Builder.Default
    private BigDecimal soLuongNhap = BigDecimal.ZERO;
    
    @Column(name = "so_luong_xuat", nullable = false, precision = 18, scale = 4)
    @Builder.Default
    private BigDecimal soLuongXuat = BigDecimal.ZERO;
    
    @Column(name = "so_luong_cuoi", nullable = false, precision = 18, scale = 4)
    private BigDecimal soLuongCuoi; // Số lượng cuối kỳ
    
    @Column(name = "gia_von_dau", nullable = false, precision = 18, scale = 2)
    private BigDecimal giaVonDau;
    
    @Column(name = "gia_von_nhap", precision = 18, scale = 2)
    private BigDecimal giaVonNhap;
    
    @Column(name = "gia_von_xuat", precision = 18, scale = 2)
    private BigDecimal giaVonXuat;
    
    @Column(name = "gia_von_cuoi", precision = 18, scale = 2)
    private BigDecimal giaVonCuoi;
    
    @Column(name = "phuong_thuc_tinh_gia", nullable = false, length = 20)
    private String phuongThucTinhGia; // FIFO, LIFO, TRUNG_BINH
    
    @Column(name = "don_vi_tinh", length = 20)
    private String donViTinh; // Cái, bộ, kg, lít, etc.
    
    @Column(name = "ngay_sap_het_han")
    private java.time.LocalDate ngaySapHetHan;
    
    @Column(name = "created_by", nullable = false)
    private Long createdBy;
    
    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;
    
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
        if (soLuongCuoi == null && soLuongDau != null) {
            soLuongCuoi = soLuongDau;
        }
        if (giaVonCuoi == null && giaVonDau != null) {
            giaVonCuoi = giaVonDau;
        }
    }
    
    @PreUpdate
    protected void onUpdate() {
        updatedAt = LocalDateTime.now();
    }
}
