package com.tonvq.accountingerp.infrastructure.web.controller;

import com.tonvq.accountingerp.application.dto.*;
import com.tonvq.accountingerp.application.service.DonHangApplicationService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.*;
import javax.validation.Valid;
import java.util.List;

/**
 * REST Controller - Đơn hàng (Sales Order)
 * Lifecycle: DRAFT → CONFIRMED → SHIPPING → DELIVERED → PAID
 */
@RestController
@RequestMapping("/api/don-hang")
@RequiredArgsConstructor
@Validated
@Slf4j
@Tag(name = "Đơn hàng", description = "Quản lý đơn hàng bán")
public class DonHangController {
    
    private final DonHangApplicationService service;
    
    @PostMapping
    @Operation(summary = "Tạo đơn hàng mới", description = "Tạo đơn hàng ở trạng thái DRAFT")
    public ResponseEntity<DonHangResponse> create(@Valid @RequestBody DonHangCreateRequest request) {
        log.info("Creating new don hang: {}", request.getMaDonHang());
        DonHangResponse response = service.createDonHang(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }
    
    @GetMapping("/{id}")
    @Operation(summary = "Lấy đơn hàng theo ID")
    public ResponseEntity<DonHangResponse> getById(@PathVariable Long id) {
        log.info("Getting don hang by id: {}", id);
        DonHangResponse response = service.getDonHangById(id);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/ma/{maDonHang}")
    @Operation(summary = "Lấy đơn hàng theo mã")
    public ResponseEntity<DonHangResponse> getByMa(@PathVariable String maDonHang) {
        log.info("Getting don hang by ma: {}", maDonHang);
        DonHangResponse response = service.getDonHangByMa(maDonHang);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/trang-thai/{trangThai}")
    @Operation(summary = "Lấy đơn hàng theo trạng thái")
    public ResponseEntity<List<DonHangResponse>> getByTrangThai(@PathVariable String trangThai) {
        log.info("Getting don hang by trang thai: {}", trangThai);
        List<DonHangResponse> response = service.getDonHangByTrangThai(trangThai);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/chua-thanh-toan")
    @Operation(summary = "Lấy đơn hàng chưa thanh toán")
    public ResponseEntity<List<DonHangResponse>> getUnpaid() {
        log.info("Getting unpaid don hang");
        List<DonHangResponse> response = service.getUnpaidDonHang();
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/confirm")
    @Operation(summary = "Xác nhận đơn hàng", description = "DRAFT → CONFIRMED")
    public ResponseEntity<DonHangResponse> confirm(@PathVariable Long id, @Valid @RequestBody DonHangConfirmRequest request) {
        log.info("Confirming don hang: {}", id);
        request.setDonHangId(id);
        DonHangResponse response = service.confirmDonHang(request);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/ship")
    @Operation(summary = "Giao hàng", description = "CONFIRMED → DELIVERED")
    public ResponseEntity<DonHangResponse> ship(@PathVariable Long id, @Valid @RequestBody DonHangShipRequest request) {
        log.info("Shipping don hang: {}", id);
        request.setDonHangId(id);
        DonHangResponse response = service.shipDonHang(request);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/payment")
    @Operation(summary = "Ghi nhận thanh toán", description = "Ghi nhận thanh toán đơn hàng")
    public ResponseEntity<DonHangResponse> recordPayment(@PathVariable Long id, @Valid @RequestBody DonHangPaymentRequest request) {
        log.info("Recording payment for don hang: {}", id);
        request.setDonHangId(id);
        DonHangResponse response = service.recordPayment(request);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/calculate-vat")
    @Operation(summary = "Tính VAT", description = "Tính và cập nhật VAT cho đơn hàng")
    public ResponseEntity<DonHangResponse> calculateVAT(@PathVariable Long id, @RequestParam Double vat) {
        log.info("Calculating VAT for don hang: {}", id);
        DonHangResponse response = service.calculateVAT(id, new java.math.BigDecimal(vat));
        return ResponseEntity.ok(response);
    }
}
