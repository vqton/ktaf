package com.tonvq.accountingerp.infrastructure.web.controller;

import com.tonvq.accountingerp.application.dto.KhachHangCreateRequest;
import com.tonvq.accountingerp.application.dto.KhachHangResponse;
import com.tonvq.accountingerp.application.service.KhachHangApplicationService;
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
 * REST Controller - Khách hàng (Customer)
 */
@RestController
@RequestMapping("/api/khach-hang")
@RequiredArgsConstructor
@Validated
@Slf4j
@Tag(name = "Khách hàng", description = "Quản lý khách hàng")
public class KhachHangController {
    
    private final KhachHangApplicationService service;
    
    @PostMapping
    @Operation(summary = "Tạo khách hàng", description = "Tạo khách hàng mới")
    public ResponseEntity<KhachHangResponse> create(@Valid @RequestBody KhachHangCreateRequest request) {
        log.info("Creating new khach hang: {}", request.getMaKhachHang());
        KhachHangResponse response = service.createKhachHang(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }
    
    @GetMapping("/{id}")
    @Operation(summary = "Lấy khách hàng theo ID")
    public ResponseEntity<KhachHangResponse> getById(@PathVariable Long id) {
        log.info("Getting khach hang by id: {}", id);
        KhachHangResponse response = service.getKhachHangById(id);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/ma/{maKhachHang}")
    @Operation(summary = "Lấy khách hàng theo mã")
    public ResponseEntity<KhachHangResponse> getByMa(@PathVariable String maKhachHang) {
        log.info("Getting khach hang by ma: {}", maKhachHang);
        KhachHangResponse response = service.getKhachHangByMa(maKhachHang);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping
    @Operation(summary = "Lấy danh sách khách hàng")
    public ResponseEntity<List<KhachHangResponse>> getAll() {
        log.info("Getting all khach hang");
        List<KhachHangResponse> response = service.getAllKhachHang();
        return ResponseEntity.ok(response);
    }
    
    @PutMapping("/{id}")
    @Operation(summary = "Cập nhật khách hàng")
    public ResponseEntity<KhachHangResponse> update(@PathVariable Long id, @Valid @RequestBody KhachHangCreateRequest request) {
        log.info("Updating khach hang: {}", id);
        KhachHangResponse response = service.updateKhachHang(id, request);
        return ResponseEntity.ok(response);
    }
    
    @DeleteMapping("/{id}")
    @Operation(summary = "Xóa khách hàng")
    public ResponseEntity<Void> delete(@PathVariable Long id) {
        log.info("Deleting khach hang: {}", id);
        service.deleteKhachHang(id);
        return ResponseEntity.noContent().build();
    }
}
