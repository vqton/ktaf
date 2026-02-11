package com.tonvq.accountingerp.infrastructure.persistence.entity;

import lombok.*;
import javax.persistence.*;
import java.math.BigDecimal;
import java.time.LocalDateTime;

/**
 * JPA Entity - Bút toán (Journal Entry Detail)
 * Chi tiết từng dòng trong chứng từ
 */
@Entity
@Table(name = "but_toan", schema = "public", indexes = {
    @Index(name = "idx_but_toan_chung_tu", columnList = "chung_tu_id"),
    @Index(name = "idx_but_toan_tk_no", columnList = "tk_no")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class ButToanEntity {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "chung_tu_id", nullable = false)
    private ChungTuEntity chungTu;
    
    @Column(name = "tk_no", nullable = false, length = 20)
    private String tkNo; // Tài khoản nợ
    
    @Column(name = "tk_co", nullable = false, length = 20)
    private String tkCo; // Tài khoản có
    
    @Column(name = "so_tien", nullable = false, precision = 18, scale = 2)
    private BigDecimal soTien;
    
    @Column(name = "dien_giai", length = 500)
    private String dienGiai; // Diễn giải
    
    @Column(name = "so_chung_tu", length = 50)
    private String soChungTu; // Số chứng từ gốc (nếu có)
    
    @Column(name = "ngay_chung_tu")
    private java.time.LocalDate ngayChungTu;
    
    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;
    
    @Column(name = "updated_at")
    private LocalDateTime updatedAt;
    
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
