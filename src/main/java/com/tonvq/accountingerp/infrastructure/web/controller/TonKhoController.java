package com.tonvq.accountingerp.infrastructure.web.controller;

import com.tonvq.accountingerp.application.dto.*;
import com.tonvq.accountingerp.application.service.TonKhoApplicationService;
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
 * REST Controller - Tồn kho (Inventory)
 * TT 99/2025 Phụ lục II: FIFO, LIFO, Trung bình
 */
@RestController
@RequestMapping("/api/ton-kho")
@RequiredArgsConstructor
@Validated
@Slf4j
@Tag(name = "Tồn kho", description = "Quản lý kho hàng")
public class TonKhoController {
    
    private final TonKhoApplicationService service;
    
    @PostMapping
    @Operation(summary = "Tạo hàng hoá tồn kho", description = "Tạo hàng hoá mới trong kho")
    public ResponseEntity<TonKhoResponse> create(@Valid @RequestBody TonKhoCreateRequest request) {
        log.info("Creating new ton kho: {}", request.getMaSanPham());
        TonKhoResponse response = service.createTonKho(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }
    
    @GetMapping("/{id}")
    @Operation(summary = "Lấy thông tin hàng hoá")
    public ResponseEntity<TonKhoResponse> getById(@PathVariable Long id) {
        log.info("Getting ton kho by id: {}", id);
        TonKhoResponse response = service.getTonKhoById(id);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/ma/{maSanPham}")
    @Operation(summary = "Lấy hàng hoá theo mã")
    public ResponseEntity<TonKhoResponse> getByMa(@PathVariable String maSanPham) {
        log.info("Getting ton kho by ma san pham: {}", maSanPham);
        TonKhoResponse response = service.getTonKhoByMaSanPham(maSanPham);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/import")
    @Operation(summary = "Nhập hàng", description = "Ghi nhận nhập hàng vào kho")
    public ResponseEntity<TonKhoResponse> importStock(@PathVariable Long id, @Valid @RequestBody NhapHangRequest request) {
        log.info("Importing stock for ton kho: {}", id);
        request.setTonKhoId(id);
        TonKhoResponse response = service.importStock(request);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/export")
    @Operation(summary = "Xuất hàng", description = "Ghi nhận xuất hàng khỏi kho")
    public ResponseEntity<TonKhoResponse> exportStock(@PathVariable Long id, @Valid @RequestBody XuatHangRequest request) {
        log.info("Exporting stock for ton kho: {}", id);
        request.setTonKhoId(id);
        TonKhoResponse response = service.exportStock(request);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/calculate-cost")
    @Operation(summary = "Tính giá vốn", description = "Tính giá vốn theo FIFO, LIFO, hoặc Trung bình")
    public ResponseEntity<TonKhoResponse> calculateCost(@PathVariable Long id, @Valid @RequestBody TinhGiaVonRequest request) {
        log.info("Calculating cost for ton kho: {}", id);
        request.setTonKhoId(id);
        TonKhoResponse response = service.calculateCost(request);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/het-hang")
    @Operation(summary = "Lấy hàng hoá hết hàng")
    public ResponseEntity<List<TonKhoResponse>> getOutOfStock() {
        log.info("Getting out of stock products");
        List<TonKhoResponse> response = service.getOutOfStockProducts();
        return ResponseEntity.ok(response);
    }
}
