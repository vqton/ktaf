package com.tonvq.accountingerp.infrastructure.web.controller;

import com.tonvq.accountingerp.application.dto.HoaDonCreateRequest;
import com.tonvq.accountingerp.application.dto.HoaDonResponse;
import com.tonvq.accountingerp.application.service.HoaDonApplicationService;
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
 * REST Controller - Hóa đơn (Invoice)
 * Lifecycle: DRAFT → ISSUED → CANCELLED
 */
@RestController
@RequestMapping("/api/hoa-don")
@RequiredArgsConstructor
@Validated
@Slf4j
@Tag(name = "Hóa đơn", description = "Quản lý hóa đơn")
public class HoaDonController {
    
    private final HoaDonApplicationService service;
    
    @PostMapping
    @Operation(summary = "Tạo hóa đơn", description = "Tạo hóa đơn mới")
    public ResponseEntity<HoaDonResponse> create(@Valid @RequestBody HoaDonCreateRequest request) {
        log.info("Creating new hoa don: {}", request.getMaHoaDon());
        HoaDonResponse response = service.createHoaDon(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }
    
    @GetMapping("/{id}")
    @Operation(summary = "Lấy hóa đơn theo ID")
    public ResponseEntity<HoaDonResponse> getById(@PathVariable Long id) {
        log.info("Getting hoa don by id: {}", id);
        HoaDonResponse response = service.getHoaDonById(id);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/trang-thai/{trangThai}")
    @Operation(summary = "Lấy hóa đơn theo trạng thái")
    public ResponseEntity<List<HoaDonResponse>> getByTrangThai(@PathVariable String trangThai) {
        log.info("Getting hoa don by trang thai: {}", trangThai);
        List<HoaDonResponse> response = service.getHoaDonByTrangThai(trangThai);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/publish")
    @Operation(summary = "Phát hành hóa đơn", description = "DRAFT → ISSUED")
    public ResponseEntity<HoaDonResponse> publish(@PathVariable Long id) {
        log.info("Publishing hoa don: {}", id);
        HoaDonResponse response = service.publishHoaDon(id);
        return ResponseEntity.ok(response);
    }
    
    @DeleteMapping("/{id}")
    @Operation(summary = "Hủy hóa đơn", description = "ISSUED → CANCELLED")
    public ResponseEntity<HoaDonResponse> cancel(@PathVariable Long id, @RequestParam String reason) {
        log.info("Cancelling hoa don: {}", id);
        HoaDonResponse response = service.cancelHoaDon(id, reason);
        return ResponseEntity.ok(response);
    }
}
