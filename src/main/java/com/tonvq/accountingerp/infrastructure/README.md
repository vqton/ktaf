# Infrastructure Layer - AccountingERP

Tầng Infrastructure cung cấp các thành phần kỹ thuật để kết nối Domain Layer và Application Layer với các công nghệ Spring Boot.

## 📁 Cấu trúc Thư mục

```
infrastructure/
├── persistence/
│   ├── entity/               # JPA Entities (ChungTuEntity, DonHangEntity, ...)
│   │   ├── ChungTuEntity.java
│   │   ├── DonHangEntity.java
│   │   ├── TonKhoEntity.java
│   │   ├── HoaDonEntity.java
│   │   ├── HopDongDichVuEntity.java
│   │   ├── KhachHangEntity.java
│   │   ├── UserEntity.java
│   │   └── ButToanEntity.java
│   └── repository/           # Spring Data JPA Repositories & Adapters
│       ├── BaseRepository.java
│       ├── JpaChungTuRepository.java
│       ├── JpaDonHangRepository.java
│       ├── JpaTonKhoRepository.java
│       ├── JpaHoaDonRepository.java
│       ├── JpaHopDongDichVuRepository.java
│       ├── JpaKhachHangRepository.java
│       ├── JpaUserRepository.java
│       └── ChungTuRepositoryAdapter.java  # Adapter pattern
├── web/
│   ├── controller/           # REST Controllers
│   │   ├── ChungTuController.java
│   │   ├── DonHangController.java
│   │   ├── TonKhoController.java
│   │   ├── HopDongDichVuController.java
│   │   ├── HoaDonController.java
│   │   └── KhachHangController.java
│   ├── filter/              # JWT Authentication Filter
│   └── dto/                 # DTO for web layer (if needed)
├── config/
│   ├── SecurityConfig.java         # Spring Security + JWT
│   ├── JpaConfig.java              # JPA Configuration
│   ├── ThymeleafConfig.java        # Template Engine
│   ├── SwaggerConfig.java          # OpenAPI 3.0
│   └── WebConfig.java              # Web MVC Configuration
└── exception/
    ├── GlobalExceptionHandler.java  # @RestControllerAdvice
    └── ErrorResponse.java           # Standard Error DTO
```

## 🔌 Persistence Layer

### JPA Entities

Mỗi entity tương ứng với một domain entity:

- **ChungTuEntity** - Chứng từ (Document)
  - Lifecycle: DRAFT → APPROVED → POSTED → LOCKED
  - Indexes: ma_chung_tu (unique), trang_thai, ngay_chung_tu
  - Relationships: OneToMany với ButToanEntity

- **DonHangEntity** - Đơn hàng (Sales Order)
  - Lifecycle: DRAFT → CONFIRMED → SHIPPING → DELIVERED → PAID
  - Payment tracking: tienDaThanhToan, tienConNo
  - VAT calculation: tyLeVAT, tienVAT

- **TonKhoEntity** - Tồn kho (Inventory)
  - TT 99/2025 Phụ lục II: FIFO, LIFO, TRUNG_BINH
  - Quantity tracking: soLuongDau, soLuongNhap, soLuongXuat, soLuongCuoi
  - Cost tracking: giaVonDau, giaVonNhap, giaVonXuat, giaVonCuoi

- **HoaDonEntity** - Hóa đơn (Invoice)
  - Lifecycle: DRAFT → ISSUED → CANCELLED
  - OneToOne relationship với DonHangEntity

- **HopDongDichVuEntity** - Hợp đồng dịch vụ (Service Contract)
  - Lifecycle: DRAFT → ACTIVE → IN_PROGRESS → COMPLETED
  - VAS 14/15: Revenue recognition by milestone or % completion

- **KhachHangEntity** - Khách hàng (Customer)
  - OneToMany với DonHangEntity và HopDongDichVuEntity

- **UserEntity** - User (for audit & security)
  - Role-based access: ROLE_ADMIN, ROLE_ACCOUNTANT, ROLE_VIEWER

### Spring Data JPA Repositories

Pattern: **Base Interface → Spring Data Interface → Repository Adapter**

```java
// 1. Base Interface (provided)
public interface BaseRepository<T, ID> extends JpaRepository<T, ID> {
    List<T> findAllByIsDeletedFalse();
}

// 2. Spring Data Interface (custom queries)
public interface JpaChungTuRepository extends BaseRepository<ChungTuEntity, Long> {
    Optional<ChungTuEntity> findByMaChungTu(String maChungTu);
    List<ChungTuEntity> findByTrangThai(String trangThai);
    List<ChungTuEntity> findByDateRange(LocalDate startDate, LocalDate endDate);
}

// 3. Repository Adapter (Adapter Pattern)
@Repository
public class ChungTuRepositoryAdapter implements ChungTuRepository {
    private final JpaChungTuRepository jpaRepository;
    
    @Override
    public ChungTu save(ChungTu domain) {
        ChungTuEntity entity = toDomainModel(domain);
        ChungTuEntity saved = jpaRepository.save(entity);
        return toEntity(saved);
    }
    // ... conversion methods ...
}
```

**Lợi ích của Adapter Pattern:**
- Domain layer không phụ thuộc vào Spring/JPA
- Dễ dàng thay đổi persistence technology
- Clear separation of concerns

## 🌐 Web Layer

### REST Controllers

Mỗi controller tương ứng với một domain aggregate:

#### ChungTuController
```
POST   /api/chung-tu              - Tạo chứng từ
GET    /api/chung-tu/{id}         - Lấy chứng từ theo ID
GET    /api/chung-tu/ma/{ma}      - Lấy chứng từ theo mã
GET    /api/chung-tu/trang-thai/  - Lấy chứng từ theo trạng thái
POST   /api/chung-tu/{id}/approve - Duyệt chứng từ
POST   /api/chung-tu/{id}/post    - Phát hành chứng từ
POST   /api/chung-tu/{id}/lock    - Khóa chứng từ
DELETE /api/chung-tu/{id}         - Hủy chứng từ
```

#### DonHangController
```
POST   /api/don-hang                  - Tạo đơn hàng
GET    /api/don-hang/{id}             - Lấy đơn hàng
GET    /api/don-hang/trang-thai/{...} - Lấy theo trạng thái
GET    /api/don-hang/chua-thanh-toan  - Lấy chưa thanh toán
POST   /api/don-hang/{id}/confirm     - Xác nhận
POST   /api/don-hang/{id}/ship        - Giao hàng
POST   /api/don-hang/{id}/payment     - Ghi nhận thanh toán
POST   /api/don-hang/{id}/calculate-vat - Tính VAT
```

#### TonKhoController
```
POST   /api/ton-kho                - Tạo hàng hoá
GET    /api/ton-kho/{id}           - Lấy thông tin hàng hoá
GET    /api/ton-kho/ma/{ma}        - Lấy theo mã sản phẩm
POST   /api/ton-kho/{id}/import    - Nhập hàng
POST   /api/ton-kho/{id}/export    - Xuất hàng
POST   /api/ton-kho/{id}/calculate-cost - Tính giá vốn (FIFO/LIFO/AVG)
GET    /api/ton-kho/het-hang       - Lấy hàng hết
```

#### HopDongDichVuController
```
POST   /api/hop-dong-dich-vu                  - Tạo hợp đồng
GET    /api/hop-dong-dich-vu/{id}             - Lấy hợp đồng
GET    /api/hop-dong-dich-vu/trang-thai/...  - Lấy theo trạng thái
POST   /api/hop-dong-dich-vu/{id}/activate   - Kích hoạt
POST   /api/hop-dong-dich-vu/{id}/update-progress - Cập nhật tiến độ
POST   /api/hop-dong-dich-vu/{id}/recognize-revenue - Công nhận doanh thu
POST   /api/hop-dong-dich-vu/{id}/complete   - Hoàn thành
```

#### HoaDonController & KhachHangController
```
POST   /api/hoa-don/{id}/publish     - Phát hành hóa đơn
POST   /api/khach-hang               - Tạo khách hàng
PUT    /api/khach-hang/{id}          - Cập nhật khách hàng
```

### API Documentation

Swagger UI available at: **http://localhost:8080/swagger-ui.html**

API Docs JSON: **http://localhost:8080/v3/api-docs**

## 🔐 Security Configuration

### JWT Authentication

```yaml
# application.yml
spring:
  security:
    jwt:
      secret: ${JWT_SECRET}
      expiration: 86400000  # 24 hours
```

### Role-Based Access Control

```java
@Configuration
@EnableWebSecurity
public class SecurityConfig {
    
    @Bean
    public SecurityFilterChain filterChain(HttpSecurity http) {
        http.authorizeRequests()
            .antMatchers(HttpMethod.GET, "/api/**").hasAnyRole("ADMIN", "ACCOUNTANT", "VIEWER")
            .antMatchers(HttpMethod.POST, "/api/**").hasAnyRole("ADMIN", "ACCOUNTANT")
            .antMatchers(HttpMethod.PUT, "/api/**").hasAnyRole("ADMIN", "ACCOUNTANT")
            .antMatchers(HttpMethod.DELETE, "/api/**").hasRole("ADMIN")
            .anyRequest().authenticated();
        return http.build();
    }
}
```

**Roles:**
- `ROLE_ADMIN` - Full access (read, write, delete)
- `ROLE_ACCOUNTANT` - Read and write access
- `ROLE_VIEWER` - Read-only access

## 📋 Exception Handling

### GlobalExceptionHandler

Xử lý các exceptions từ Domain Layer và trả về standardized error responses:

```java
@RestControllerAdvice
public class GlobalExceptionHandler {
    
    @ExceptionHandler(BusinessException.class)
    public ResponseEntity<ErrorResponse> handleBusinessException(BusinessException ex) {
        return ResponseEntity.badRequest().body(new ErrorResponse(...));
    }
    
    @ExceptionHandler(ResourceNotFoundException.class)
    public ResponseEntity<ErrorResponse> handleResourceNotFoundException(...) {
        return ResponseEntity.notFound().build();
    }
    
    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<ErrorResponse> handleValidationException(...) {
        // Validation errors
    }
}
```

### Error Response Format

```json
{
  "timestamp": "2026-02-11T10:30:45.123456",
  "status": 400,
  "error": "Business Error",
  "message": "Chứng từ phải ở trạng thái DRAFT để duyệt",
  "path": "/api/chung-tu/1/approve",
  "validationErrors": {
    "approvalReason": "Approval reason is required"
  }
}
```

## 🔧 Integration Architecture

```
┌─────────────────────────────────────────┐
│           HTTP Request                  │
└──────────┬──────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│      REST Controller                    │
│  (ChungTuController, DonHangController) │
│  @RestController, @RequestMapping       │
└──────────┬──────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│    Application Service                  │
│  (ChungTuApplicationService, ...)       │
│  @Service, @Transactional               │
└──────────┬──────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│    Domain Layer                         │
│  (Entities, Value Objects, Services)   │
│  Pure Java, No Framework Dependencies   │
└──────────┬──────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│    Repository Adapter                   │
│  (ChungTuRepositoryAdapter, ...)       │
│  Implements Domain Repository Interface │
└──────────┬──────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│    JPA Repository                       │
│  (JpaChungTuRepository, ...)            │
│  extends BaseRepository<Entity, ID>     │
└──────────┬──────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│    JPA Entity                           │
│  (ChungTuEntity, DonHangEntity, ...)   │
│  @Entity, @Table, @Column              │
└──────────┬──────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│    Database                             │
│  (PostgreSQL in Production)             │
│  (H2 in Development)                    │
└─────────────────────────────────────────┘
```

## 🚀 Running the Application

### Development Environment

```bash
# Using H2 in-memory database
mvn spring-boot:run -Dspring-boot.run.arguments='--spring.profiles.active=dev'
```

Visit: **http://localhost:8080**

### Production Environment

```bash
# Using PostgreSQL
mvn spring-boot:run -Dspring-boot.run.arguments='--spring.profiles.active=prod'
```

Database setup:
```sql
CREATE DATABASE accounting_erp;
CREATE USER erp_user WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON DATABASE accounting_erp TO erp_user;
```

## 📊 Database Schema

All tables created by Hibernate DDL-Auto with proper indexes:

```
chung_tu          - Vouchers/Documents
but_toan          - Journal Entry Details
don_hang          - Sales Orders
don_hang_chi_tiet - Order Line Items
hoa_don           - Invoices
hop_dong_dich_vu  - Service Contracts
ton_kho           - Inventory
khach_hang        - Customers
users             - System Users
user_roles        - User Role Mapping
```

## 🧪 Testing

### Integration Tests

Test database operations:
```java
@SpringBootTest
@ActiveProfiles("dev")
public class ChungTuRepositoryTest {
    @Autowired
    private JpaChungTuRepository repository;
    
    @Test
    public void testSaveAndFind() {
        // Test implementation
    }
}
```

### Controller Tests

Test REST endpoints:
```java
@SpringBootTest
@AutoConfigureMockMvc
public class ChungTuControllerTest {
    @Autowired
    private MockMvc mockMvc;
    
    @Test
    public void testCreateChungTu() throws Exception {
        // Test implementation
    }
}
```

## 📚 Next Steps

1. **Implement Thymeleaf Views** - Create HTML templates for web UI
2. **Add JWT Authentication** - Implement login endpoint
3. **Database Migrations** - Use Flyway for schema management
4. **Reporting** - Implement B01-B09 reports (TT 99 Phụ lục IV)
5. **E-invoicing** - Integrate eTax API

## 📝 Configuration Files

### application.yml
```yaml
spring:
  datasource:
    url: jdbc:postgresql://localhost:5432/accounting_erp
    username: postgres
    password: postgres
  jpa:
    hibernate:
      ddl-auto: validate
    properties:
      hibernate:
        dialect: org.hibernate.dialect.PostgreSQLDialect
        format_sql: true
```

### application-dev.yml
```yaml
spring:
  datasource:
    url: jdbc:h2:mem:accounting_erp_dev
  jpa:
    hibernate:
      ddl-auto: create-drop
    database-platform: org.hibernate.dialect.H2Dialect
```

## 📞 Support & References

- **GitHub**: https://github.com/vqton/ktaf
- **Documentation**: See INFRASTRUCTURE_LAYER_GUIDE.md
- **Compliance**: TT 99/2025/TT-BTC, VAS 14/15, TT 48/2019

---

**Status**: ✅ Infrastructure Layer Complete
**Last Updated**: February 11, 2026
