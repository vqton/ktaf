package com.tonvq.accountingerp.infrastructure.exception;

import lombok.*;
import java.time.LocalDateTime;
import java.util.Map;

/**
 * Standard Error Response DTO
 * Được trả về từ GlobalExceptionHandler
 */
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class ErrorResponse {
    private LocalDateTime timestamp;
    private int status;
    private String error;
    private String message;
    private String path;
    private Map<String, String> validationErrors;
}
