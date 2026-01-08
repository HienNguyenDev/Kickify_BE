# User CRUD Implementation - Clean Architecture

## 📋 Tổng quan Flow Clean Architecture

### Kiến trúc phân tầng:
```
┌─────────────────────────────────────────────────────────┐
│                   API Layer (Kickify.Api)               │
│  - Controllers: Nhận HTTP requests                      │
│  - Request DTOs: Map input từ client                   │
│  - Extension Methods: Match Result pattern              │
└────────────────┬────────────────────────────────────────┘
                 │ ↓ MediatR (CQRS)
┌────────────────┴────────────────────────────────────────┐
│          Application Layer (Kickify.Application)         │
│  - Commands/Queries: Define use cases                   │
│  - Handlers: Business logic implementation              │
│  - Validators: FluentValidation rules                   │
│  - Abstractions: Interfaces for dependencies            │
└────────────────┬────────────────────────────────────────┘
                 │ ↓ Dependency Injection
┌────────────────┴────────────────────────────────────────┐
│        Infrastructure Layer (Kickify.Infrastructure)     │
│  - Repositories: Database access implementation         │
│  - DbContext: Entity Framework Core                     │
│  - Authentication: JWT, Password hashing                │
└────────────────┬────────────────────────────────────────┘
                 │ ↓ Domain Models
┌────────────────┴────────────────────────────────────────┐
│              Domain Layer (Kickify.Domain)              │
│  - Entities: Domain models (User, etc.)                │
│  - Errors: Domain-specific error definitions            │
│  - Enums: Domain enumerations                           │
│  - Common: Result pattern, base entities                │
└─────────────────────────────────────────────────────────┘
```

## 🔄 Request Flow Details

### 1. CREATE USER Flow
```
Client HTTP POST /api/users
   ↓
UsersController.CreateUser(CreateUserRequest)
   ↓ Map to Command
CreateUserCommand { Email, Password, FullName, ... }
   ↓ MediatR.Send()
CreateUserCommandValidator (FluentValidation)
   ├─ Email: NotEmpty, EmailAddress, MaxLength(255)
   ├─ Password: NotEmpty, MinLength(6), MaxLength(100)
   ├─ Phone: Regex validation (optional)
   ├─ DateOfBirth: Must be in past (optional)
   └─ Role: IsInEnum
   ↓ If Valid
CreateUserCommandHandler.Handle()
   ├─ Check email exists (UserRepository.IsEmailExistsAsync)
   ├─ Hash password (IPasswordHasher)
   ├─ Create User entity
   ├─ Add to repository (UserRepository.AddAsync)
   └─ Save changes (IUnitOfWork.SaveChangesAsync)
   ↓ Return Result<CreateUserCommandResponse>
Controller maps Result to IResult (MatchOk())
   ↓
HTTP Response: 200 OK + UserData or 400/409 + Error
```

### 2. UPDATE USER Flow
```
Client HTTP PUT /api/users/{userId}
   ↓
UsersController.UpdateUser(userId, UpdateUserRequest)
   ↓
UpdateUserCommand + UpdateUserCommandValidator
   ↓
UpdateUserCommandHandler
   ├─ Get user by ID (UserRepository.GetByIdAsync)
   ├─ Check if user exists (NotFound error if null)
   ├─ Update properties (FullName, Phone, Avatar, etc.)
   ├─ Update timestamp (UpdatedAt = DateTime.UtcNow)
   ├─ Mark as modified (UserRepository.Update)
   └─ Save changes (UnitOfWork)
   ↓
Return Result → HTTP Response
```

### 3. DELETE USER Flow (Soft Delete)
```
Client HTTP DELETE /api/users/{userId}
   ↓
UsersController.DeleteUser(userId)
   ↓
DeleteUserCommand + DeleteUserCommandValidator
   ↓
DeleteUserCommandHandler
   ├─ Get user by ID
   ├─ Check if exists
   ├─ Set DeletedAt = DateTime.UtcNow (Soft Delete)
   ├─ Set IsActive = false
   └─ Save changes
   ↓
Return Result → HTTP Response
```

### 4. GET USER BY ID Flow
```
Client HTTP GET /api/users/{userId}
   ↓
UsersController.GetUserById(userId)
   ↓
GetUserByIdQuery + GetUserByIdQueryValidator
   ↓
GetUserByIdQueryHandler
   ├─ Get user (UserRepository.GetByIdAsync)
   └─ Map to response DTO
   ↓
Return Result → HTTP Response
```

### 5. GET ALL USERS Flow (Pagination + Filters)
```
Client HTTP GET /api/users?page=1&pageSize=10&role=Player&searchTerm=john
   ↓
UsersController.GetAllUsers(page, pageSize, role, isActive, searchTerm)
   ↓
GetAllUsersQuery + GetAllUsersQueryValidator
   ├─ Validate: Page > 0
   ├─ Validate: PageSize 1-100
   └─ Validate: Role IsInEnum (if provided)
   ↓
GetAllUsersQueryHandler
   ├─ Call UserRepository.GetPagedUsersAsync()
   │   ├─ Filter by Role (if provided)
   │   ├─ Filter by IsActive (if provided)
   │   ├─ Search by Email/FullName/Phone (if searchTerm)
   │   ├─ OrderBy CreatedAt DESC
   │   ├─ Skip/Take for pagination
   │   └─ Return (Users[], TotalCount)
   └─ Map to GetAllUsersQueryResponse
       ├─ Users: List<UserDto>
       ├─ TotalCount
       ├─ Page
       ├─ PageSize
       └─ TotalPages (calculated)
   ↓
Return Result → HTTP Response
```

## 📁 Cấu trúc Files đã tạo

### Application Layer
```
Kickify.Application/Features/Users/
├── Commands/
│   ├── CreateUser/
│   │   ├── CreateUserCommand.cs
│   │   ├── CreateUserCommandHandler.cs
│   │   ├── CreateUserCommandResponse.cs
│   │   └── CreateUserCommandValidator.cs
│   ├── UpdateUser/
│   │   ├── UpdateUserCommand.cs
│   │   ├── UpdateUserCommandHandler.cs
│   │   ├── UpdateUserCommandResponse.cs
│   │   └── UpdateUserCommandValidator.cs
│   └── DeleteUser/
│       ├── DeleteUserCommand.cs
│       ├── DeleteUserCommandHandler.cs
│       ├── DeleteUserCommandResponse.cs
│       └── DeleteUserCommandValidator.cs
└── Queries/
    ├── GetUserById/
    │   ├── GetUserByIdQuery.cs
    │   ├── GetUserByIdQueryHandler.cs
    │   ├── GetUserByIdQueryResponse.cs
    │   └── GetUserByIdQueryValidator.cs
    └── GetAllUsers/
        ├── GetAllUsersQuery.cs
        ├── GetAllUsersQueryHandler.cs
        ├── GetAllUsersQueryResponse.cs
        └── GetAllUsersQueryValidator.cs
```

### API Layer
```
Kickify.Api/
├── Controllers/
│   └── UsersController.cs
└── Requests/
    ├── CreateUserRequest.cs
    └── UpdateUserRequest.cs
```

### Repository Updates
```
Kickify.Application/Abstractions/Repositories/
└── IUserRepository.cs (đã mở rộng)
    ├── GetPagedUsersAsync() - NEW
    └── GetUserWithDetailsAsync() - NEW

Kickify.Infrastructure/Repositories/
└── UserRepository.cs (đã implement)
    ├── GetPagedUsersAsync() - Pagination + filters
    └── GetUserWithDetailsAsync() - Include related entities
```

### Domain Updates
```
Kickify.Domain/Errors/
└── UserErrors.cs (đã bổ sung)
    ├── InvalidEmail
    ├── InvalidPassword
    ├── InvalidPhoneNumber
    ├── InvalidDateOfBirth
    ├── UserAlreadyDeleted
    ├── CannotDeleteActiveUser
    ├── UpdateFailed
    ├── CreateFailed
    └── DeleteFailed
```

## 🔐 Validation Rules

### CreateUserCommand
- **Email**: Required, valid email format, max 255 chars
- **Password**: Required, min 6 chars, max 100 chars
- **FullName**: Optional, max 255 chars
- **Phone**: Optional, must match international format regex `^\+?[1-9]\d{1,14}$`
- **AvatarUrl**: Optional, must be valid HTTP/HTTPS URL
- **Bio**: Optional, max 500 chars
- **DateOfBirth**: Optional, must be in past, not more than 120 years ago
- **Role**: Must be valid UserRole enum

### UpdateUserCommand
- **UserId**: Required (non-empty GUID)
- **FullName, Phone, AvatarUrl, Bio**: Same rules as Create
- **Gender**: Optional, must be valid Gender enum if provided

### DeleteUserCommand
- **UserId**: Required (non-empty GUID)

### GetUserByIdQuery
- **UserId**: Required (non-empty GUID)

### GetAllUsersQuery
- **Page**: Must be > 0
- **PageSize**: Must be > 0 and ≤ 100
- **Role**: Optional, must be valid enum if provided
- **SearchTerm**: Optional, max 255 chars

## 🗄️ Repository Logic

### GetPagedUsersAsync
```csharp
// Filters
- Exclude soft-deleted (WHERE DeletedAt IS NULL)
- Filter by Role (if provided)
- Filter by IsActive status (if provided)
- Search in Email, FullName, Phone (case-insensitive, if searchTerm provided)

// Ordering & Pagination
- ORDER BY CreatedAt DESC
- SKIP (page - 1) * pageSize
- TAKE pageSize

// Returns
- (IEnumerable<User> users, int totalCount)
```

### Key Methods
- `GetByIdAsync(Guid id)`: Get single user by ID
- `IsEmailExistsAsync(string email)`: Check email uniqueness
- `GetPagedUsersAsync(...)`: Paginated list with filters
- `GetUserWithDetailsAsync(Guid userId)`: Include PlayerProfile, NotificationPreference

## 📡 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Get all users (paginated, filtered) |
| GET | `/api/users/{userId}` | Get user by ID |
| POST | `/api/users` | Create new user |
| PUT | `/api/users/{userId}` | Update user |
| DELETE | `/api/users/{userId}` | Soft delete user |

### Example Requests

#### Create User
```json
POST /api/users
{
  "email": "john.doe@example.com",
  "password": "SecurePass123",
  "fullName": "John Doe",
  "phone": "+84901234567",
  "avatarUrl": "https://example.com/avatar.jpg",
  "bio": "Football enthusiast",
  "dateOfBirth": "1990-05-15",
  "gender": 0,
  "role": 0
}
```

#### Update User
```json
PUT /api/users/{userId}
{
  "fullName": "John Doe Updated",
  "phone": "+84901234568",
  "bio": "Updated bio"
}
```

#### Get All Users (with filters)
```
GET /api/users?page=1&pageSize=20&role=0&isActive=true&searchTerm=john
```

## ⚡ Key Design Patterns

### 1. CQRS (Command Query Responsibility Segregation)
- **Commands**: Modify state (Create, Update, Delete)
- **Queries**: Read state (GetById, GetAll)
- Separate models for read and write operations

### 2. Mediator Pattern
- Using MediatR library
- Decouples controllers from handlers
- Central request/response pipeline

### 3. Repository Pattern
- Abstracts data access
- `IUserRepository` interface in Application
- Implementation in Infrastructure

### 4. Unit of Work Pattern
- `IUnitOfWork` manages transactions
- Single `SaveChangesAsync()` call per operation

### 5. Result Pattern
- `Result<T>` for operation outcomes
- Type-safe error handling
- No exceptions for business logic failures

### 6. Validator Pattern
- FluentValidation library
- Declarative validation rules
- Automatic validation in MediatR pipeline

## 🛡️ Error Handling

Tất cả errors trả về dạng:
```json
{
  "type": "NotFound|Validation|Conflict|Problem",
  "code": "Users.NotFound",
  "description": "The user with the Id = '...' was not found"
}
```

## 🔍 Best Practices Đã Áp Dụng

✅ Separation of Concerns: Mỗi layer có trách nhiệm riêng
✅ Dependency Inversion: Application layer không phụ thuộc Infrastructure
✅ Single Responsibility: Mỗi class/file có một nhiệm vụ duy nhất
✅ Open/Closed Principle: Dễ mở rộng mà không sửa code cũ
✅ Validation ở Application layer (business rules)
✅ Soft Delete thay vì Hard Delete
✅ Async/await cho tất cả database operations
✅ Pagination cho list queries (tránh load toàn bộ data)
✅ AsNoTracking() cho read-only queries (performance)
✅ CancellationToken support (graceful shutdown)
✅ Comprehensive validation rules với FluentValidation
✅ Proper error messages cho từng trường hợp

## 🚀 Next Steps (Tùy chọn mở rộng)

1. **Authentication/Authorization**
   - Thêm `[Authorize]` attributes
   - Role-based access control
   - JWT token validation

2. **Advanced Features**
   - Change password endpoint
   - Email verification workflow
   - Profile picture upload
   - User activity logs
   - Bulk operations

3. **Performance**
   - Caching với Redis
   - Database indexes
   - Query optimization

4. **Testing**
   - Unit tests cho Handlers
   - Integration tests cho Repositories
   - API tests cho Controllers
