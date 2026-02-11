package com.tonvq.accountingerp.infrastructure.web.controller;

import com.tonvq.accountingerp.application.dto.*;
import com.tonvq.accountingerp.application.service.HopDongDichVuApplicationService;
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
 * REST Controller - Hợp đồng dịch vụ (Service Contract)
 * VAS 14/15: Công nhận doanh thu
 */
@RestController
@RequestMapping("/api/hop-dong-dich-vu")
@RequiredArgsConstructor
@Validated
@Slf4j
@Tag(name = "Hợp đồng dịch vụ", description = "Quản lý hợp đồng dịch vụ")
public class HopDongDichVuController {
    
    private final HopDongDichVuApplicationService service;
    
    @PostMapping
    @Operation(summary = "Tạo hợp đồng", description = "Tạo hợp đồng dịch vụ mới")
    public ResponseEntity<HopDongDichVuResponse> create(@Valid @RequestBody HopDongDichVuCreateRequest request) {
        log.info("Creating new hop dong dich vu: {}", request.getMaHopDong());
        HopDongDichVuResponse response = service.createHopDong(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }
    
    @GetMapping("/{id}")
    @Operation(summary = "Lấy hợp đồng theo ID")
    public ResponseEntity<HopDongDichVuResponse> getById(@PathVariable Long id) {
        log.info("Getting hop dong by id: {}", id);
        HopDongDichVuResponse response = service.getHopDongById(id);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/trang-thai/{trangThai}")
    @Operation(summary = "Lấy hợp đồng theo trạng thái")
    public ResponseEntity<List<HopDongDichVuResponse>> getByTrangThai(@PathVariable String trangThai) {
        log.info("Getting hop dong by trang thai: {}", trangThai);
        List<HopDongDichVuResponse> response = service.getHopDongByTrangThai(trangThai);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/activate")
    @Operation(summary = "Kích hoạt hợp đồng", description = "DRAFT → ACTIVE")
    public ResponseEntity<HopDongDichVuResponse> activate(@PathVariable Long id) {
        log.info("Activating hop dong: {}", id);
        HopDongDichVuResponse response = service.activateHopDong(id);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/update-progress")
    @Operation(summary = "Cập nhật tiến độ", description = "ACTIVE → IN_PROGRESS")
    public ResponseEntity<HopDongDichVuResponse> updateProgress(@PathVariable Long id, @Valid @RequestBody HopDongDichVuProgressRequest request) {
        log.info("Updating progress for hop dong: {}", id);
        request.setHopDongId(id);
        HopDongDichVuResponse response = service.updateProgress(request);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/recognize-revenue")
    @Operation(summary = "Công nhận doanh thu", description = "Công nhận doanh thu theo VAS 14/15")
    public ResponseEntity<HopDongDichVuResponse> recognizeRevenue(@PathVariable Long id) {
        log.info("Recognizing revenue for hop dong: {}", id);
        HopDongDichVuResponse response = service.recognizeRevenue(id);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/complete")
    @Operation(summary = "Hoàn thành hợp đồng", description = "IN_PROGRESS → COMPLETED")
    public ResponseEntity<HopDongDichVuResponse> complete(@PathVariable Long id) {
        log.info("Completing hop dong: {}", id);
        HopDongDichVuResponse response = service.completeHopDong(id);
        return ResponseEntity.ok(response);
    }
}
