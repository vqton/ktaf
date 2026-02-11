package com.tonvq.accountingerp.infrastructure.persistence.entity;

import lombok.*;
import javax.persistence.*;
import java.time.LocalDateTime;
import java.util.*;

/**
 * JPA Entity - Khách hàng (Customer)
 */
@Entity
@Table(name = "khach_hang", schema = "public", indexes = {
    @Index(name = "idx_khach_hang_ma", columnList = "ma_khach_hang", unique = true),
    @Index(name = "idx_khach_hang_dien_thoai", columnList = "dien_thoai")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class KhachHangEntity {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "ma_khach_hang", nullable = false, unique = true, length = 50)
    private String maKhachHang;
    
    @Column(name = "ten_khach_hang", nullable = false, length = 500)
    private String tenKhachHang;
    
    @Column(name = "dien_thoai", length = 20)
    private String dienThoai;
    
    @Column(name = "email", length = 100)
    private String email;
    
    @Column(name = "dia_chi", length = 500)
    private String diaChi;
    
    @Column(name = "thanh_pho", length = 100)
    private String thanhPho;
    
    @Column(name = "ma_so_thue", length = 20)
    private String maSoThue;
    
    @Column(name = "nguoi_dai_dien", length = 200)
    private String nguoiDaiDien;
    
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
    
    @OneToMany(mappedBy = "khachHang", fetch = FetchType.LAZY)
    private List<DonHangEntity> donHangs = new ArrayList<>();
    
    @OneToMany(mappedBy = "khachHang", fetch = FetchType.LAZY)
    private List<HopDongDichVuEntity> hopDongDichVus = new ArrayList<>();
    
    @PrePersist
    protected void onCreate() {
        createdAt = LocalDateTime.now();
        updatedAt = LocalDateTime.now();
    }
    
    @PreUpdate
    protected void onUpdate() {
        updatedAt = LocalDateTime.now();
    }
}
