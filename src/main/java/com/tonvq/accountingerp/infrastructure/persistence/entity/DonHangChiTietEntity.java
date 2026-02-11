package com.tonvq.accountingerp.infrastructure.persistence.entity;

import lombok.*;
import javax.persistence.*;
import java.math.BigDecimal;
import java.time.LocalDateTime;

/**
 * JPA Entity - Chi tiết đơn hàng (Order Line Item)
 */
@Entity
@Table(name = "don_hang_chi_tiet", schema = "public", indexes = {
    @Index(name = "idx_donhang_chitiet_donhang", columnList = "don_hang_id")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class DonHangChiTietEntity {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "don_hang_id", nullable = false)
    private DonHangEntity donHang;
    
    @Column(name = "so_thu_tu", nullable = false)
    private Integer soThuTu;
    
    @Column(name = "ma_san_pham", nullable = false, length = 50)
    private String maSanPham;
    
    @Column(name = "ten_san_pham", nullable = false, length = 500)
    private String tenSanPham;
    
    @Column(name = "so_luong", nullable = false, precision = 18, scale = 2)
    private BigDecimal soLuong;
    
    @Column(name = "don_gia", nullable = false, precision = 18, scale = 2)
    private BigDecimal donGia;
    
    @Column(name = "tong_tien", nullable = false, precision = 18, scale = 2)
    private BigDecimal tongTien;
    
    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;
    
    @PrePersist
    protected void onCreate() {
        createdAt = LocalDateTime.now();
        if (tongTien == null && soLuong != null && donGia != null) {
            tongTien = soLuong.multiply(donGia);
        }
    }
}
