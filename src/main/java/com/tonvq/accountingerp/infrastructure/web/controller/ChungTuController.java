package com.tonvq.accountingerp.infrastructure.web.controller;

import com.tonvq.accountingerp.application.dto.*;
import com.tonvq.accountingerp.application.service.ChungTuApplicationService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.*;
import javax.validation.Valid;
import java.time.LocalDate;
import java.util.List;

/**
 * REST Controller - Chứng từ (Document/Voucher)
 * API endpoints: GET, POST, PUT, DELETE chứng từ
 * Tích hợp với ChungTuApplicationService
 */
@RestController
@RequestMapping("/api/chung-tu")
@RequiredArgsConstructor
@Validated
@Slf4j
@Tag(name = "Chứng từ", description = "Quản lý chứng từ kế toán")
public class ChungTuController {
    
    private final ChungTuApplicationService service;
    
    @PostMapping
    @Operation(summary = "Tạo chứng từ mới", description = "Tạo chứng từ ở trạng thái DRAFT")
    public ResponseEntity<ChungTuResponse> create(@Valid @RequestBody ChungTuCreateRequest request) {
        log.info("Creating new chung tu: {}", request.getMaChungTu());
        ChungTuResponse response = service.createChungTu(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }
    
    @GetMapping("/{id}")
    @Operation(summary = "Lấy chứng từ theo ID", description = "Lấy thông tin chứng từ theo ID")
    public ResponseEntity<ChungTuResponse> getById(@PathVariable Long id) {
        log.info("Getting chung tu by id: {}", id);
        ChungTuResponse response = service.getChungTuById(id);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/ma/{maChungTu}")
    @Operation(summary = "Lấy chứng từ theo mã", description = "Lấy thông tin chứng từ theo mã chứng từ")
    public ResponseEntity<ChungTuResponse> getByMa(@PathVariable String maChungTu) {
        log.info("Getting chung tu by ma: {}", maChungTu);
        ChungTuResponse response = service.getChungTuByMa(maChungTu);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping
    @Operation(summary = "Lấy danh sách chứng từ", description = "Lấy danh sách tất cả chứng từ")
    public ResponseEntity<List<ChungTuResponse>> getAll() {
        log.info("Getting all chung tu");
        List<ChungTuResponse> response = service.getChungTuByTrangThai("DRAFT");
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/trang-thai/{trangThai}")
    @Operation(summary = "Lấy chứng từ theo trạng thái", description = "DRAFT, APPROVED, POSTED, LOCKED")
    public ResponseEntity<List<ChungTuResponse>> getByTrangThai(@PathVariable String trangThai) {
        log.info("Getting chung tu by trang thai: {}", trangThai);
        List<ChungTuResponse> response = service.getChungTuByTrangThai(trangThai);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/approve")
    @Operation(summary = "Duyệt chứng từ", description = "Chuyển chứng từ từ DRAFT → APPROVED")
    public ResponseEntity<ChungTuResponse> approve(@PathVariable Long id, @Valid @RequestBody ChungTuApproveRequest request) {
        log.info("Approving chung tu: {}", id);
        request.setChungTuId(id);
        ChungTuResponse response = service.approveChungTu(request);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/post")
    @Operation(summary = "Phát hành chứng từ", description = "Chuyển chứng từ từ APPROVED → POSTED")
    public ResponseEntity<ChungTuResponse> post(@PathVariable Long id, @Valid @RequestBody ChungTuPostRequest request) {
        log.info("Posting chung tu: {}", id);
        request.setChungTuId(id);
        ChungTuResponse response = service.postChungTu(request);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{id}/lock")
    @Operation(summary = "Khóa chứng từ", description = "Chuyển chứng từ từ POSTED → LOCKED")
    public ResponseEntity<ChungTuResponse> lock(@PathVariable Long id, @Valid @RequestBody ChungTuLockRequest request) {
        log.info("Locking chung tu: {}", id);
        request.setChungTuId(id);
        ChungTuResponse response = service.lockChungTu(request);
        return ResponseEntity.ok(response);
    }
    
    @DeleteMapping("/{id}")
    @Operation(summary = "Hủy chứng từ", description = "Hủy chứng từ (xóa mềm)")
    public ResponseEntity<ChungTuResponse> cancel(@PathVariable Long id, 
                                                  @RequestParam String reason) {
        log.info("Cancelling chung tu: {}", id);
        ChungTuResponse response = service.cancelChungTu(id, 1L, reason);
        return ResponseEntity.ok(response);
    }
}
