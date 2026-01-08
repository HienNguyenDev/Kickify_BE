## 🧪 Testing Examples

### Sử dụng API với HTTP Client

#### 1. Create User
```http
POST https://localhost:7000/api/users
Content-Type: application/json

{
  "email": "player1@kickify.com",
  "password": "Player123!",
  "fullName": "Nguyễn Văn A",
  "phone": "+84901234567",
  "avatarUrl": "https://example.com/avatar.jpg",
  "bio": "Yêu thích bóng đá, vị trí tiền đạo",
  "dateOfBirth": "1995-03-20T00:00:00Z",
  "gender": 0,
  "role": 0
}
```

**Expected Response (201 Created):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "player1@kickify.com",
  "fullName": "Nguyễn Văn A",
  "phone": "+84901234567",
  "avatarUrl": "https://example.com/avatar.jpg",
  "role": 0,
  "createdAt": "2026-01-07T10:30:00Z"
}
```

**Error Response (409 Conflict - Email exists):**
```json
{
  "type": "Conflict",
  "code": "Users.EmailAlreadyExists",
  "description": "The provided email already exists."
}
```

#### 2. Get All Users (Paginated)
```http
GET https://localhost:7000/api/users?page=1&pageSize=10&role=0&isActive=true&searchTerm=Nguyễn
```

**Expected Response (200 OK):**
```json
{
  "users": [
    {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "player1@kickify.com",
      "fullName": "Nguyễn Văn A",
      "phone": "+84901234567",
      "avatarUrl": "https://example.com/avatar.jpg",
      "role": 0,
      "isEmailVerified": false,
      "isActive": true,
      "createdAt": "2026-01-07T10:30:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

#### 3. Get User By ID
```http
GET https://localhost:7000/api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Expected Response (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "player1@kickify.com",
  "fullName": "Nguyễn Văn A",
  "phone": "+84901234567",
  "avatarUrl": "https://example.com/avatar.jpg",
  "bio": "Yêu thích bóng đá, vị trí tiền đạo",
  "dateOfBirth": "1995-03-20T00:00:00Z",
  "gender": 0,
  "role": 0,
  "isEmailVerified": false,
  "isActive": true,
  "createdAt": "2026-01-07T10:30:00Z",
  "updatedAt": "2026-01-07T10:30:00Z"
}
```

**Error Response (404 Not Found):**
```json
{
  "type": "NotFound",
  "code": "Users.NotFound",
  "description": "The user with the Id = '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found"
}
```

#### 4. Update User
```http
PUT https://localhost:7000/api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "fullName": "Nguyễn Văn A - Updated",
  "phone": "+84907654321",
  "bio": "Cầu thủ chuyên nghiệp, vị trí tiền đạo cánh",
  "dateOfBirth": "1995-03-20T00:00:00Z",
  "gender": 0
}
```

**Expected Response (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "player1@kickify.com",
  "fullName": "Nguyễn Văn A - Updated",
  "phone": "+84907654321",
  "avatarUrl": "https://example.com/avatar.jpg",
  "bio": "Cầu thủ chuyên nghiệp, vị trí tiền đạo cánh",
  "dateOfBirth": "1995-03-20T00:00:00Z",
  "gender": 0,
  "updatedAt": "2026-01-07T11:00:00Z"
}
```

#### 5. Delete User (Soft Delete)
```http
DELETE https://localhost:7000/api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Expected Response (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "User deleted successfully",
  "deletedAt": "2026-01-07T11:15:00Z"
}
```

### Validation Error Examples

#### Invalid Email
```http
POST https://localhost:7000/api/users
Content-Type: application/json

{
  "email": "invalid-email",
  "password": "Pass123"
}
```

**Error Response (400 Bad Request):**
```json
{
  "type": "Validation",
  "errors": {
    "Email": ["Invalid email format"]
  }
}
```

#### Password Too Short
```http
POST https://localhost:7000/api/users
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "123"
}
```

**Error Response (400 Bad Request):**
```json
{
  "type": "Validation",
  "errors": {
    "Password": ["Password must be at least 6 characters"]
  }
}
```

#### Invalid Phone Number
```http
PUT https://localhost:7000/api/users/{userId}
Content-Type: application/json

{
  "phone": "abc123"
}
```

**Error Response (400 Bad Request):**
```json
{
  "type": "Validation",
  "errors": {
    "Phone": ["Invalid phone number format"]
  }
}
```

#### Invalid Date of Birth (Future date)
```http
POST https://localhost:7000/api/users
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Pass123",
  "dateOfBirth": "2030-01-01T00:00:00Z"
}
```

**Error Response (400 Bad Request):**
```json
{
  "type": "Validation",
  "errors": {
    "DateOfBirth": ["Date of birth must be in the past"]
  }
}
```

### Using Kickify.Api.http file

Add to `Kickify.Api.http`:

```http
### Variables
@baseUrl = https://localhost:7000
@userId = 3fa85f64-5717-4562-b3fc-2c963f66afa6

### Create User
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "email": "test@kickify.com",
  "password": "TestPass123",
  "fullName": "Test User",
  "role": 0
}

### Get All Users
GET {{baseUrl}}/api/users?page=1&pageSize=10

### Get All Players Only
GET {{baseUrl}}/api/users?page=1&pageSize=10&role=0&isActive=true

### Search Users
GET {{baseUrl}}/api/users?searchTerm=test&page=1&pageSize=10

### Get User By ID
GET {{baseUrl}}/api/users/{{userId}}

### Update User
PUT {{baseUrl}}/api/users/{{userId}}
Content-Type: application/json

{
  "fullName": "Updated Test User",
  "phone": "+84901234567",
  "bio": "Updated bio"
}

### Delete User
DELETE {{baseUrl}}/api/users/{{userId}}
```

## Query Parameter Reference

### GET /api/users

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | int | No | 1 | Page number (must be > 0) |
| pageSize | int | No | 10 | Items per page (1-100) |
| role | UserRole enum | No | null | Filter by role (0=Player, 1=VenueOwner, 2=Admin) |
| isActive | bool | No | null | Filter by active status |
| searchTerm | string | No | null | Search in email, fullName, phone |

### UserRole Enum Values
- `0` = Player
- `1` = VenueOwner
- `2` = Admin

### Gender Enum Values
- `0` = Male
- `1` = Female
- `2` = Other

## Testing Workflow

### 1. Test Create User Flow
```bash
# Step 1: Create a user
POST /api/users
# Save the returned userId

# Step 2: Verify user was created
GET /api/users/{userId}

# Step 3: Try creating same email (should fail)
POST /api/users # with same email
# Expected: 409 Conflict
```

### 2. Test Update User Flow
```bash
# Step 1: Create a user
POST /api/users

# Step 2: Update the user
PUT /api/users/{userId}

# Step 3: Verify changes
GET /api/users/{userId}
```

### 3. Test Search & Filter
```bash
# Create multiple users with different roles
POST /api/users # role = Player
POST /api/users # role = VenueOwner

# Test role filter
GET /api/users?role=0 # Should return only Players

# Test search
GET /api/users?searchTerm=john # Should return users matching "john"

# Test pagination
GET /api/users?page=1&pageSize=5
GET /api/users?page=2&pageSize=5
```

### 4. Test Delete Flow
```bash
# Step 1: Create a user
POST /api/users

# Step 2: Delete the user
DELETE /api/users/{userId}

# Step 3: Verify soft delete
GET /api/users/{userId}
# User still exists but DeletedAt is set, IsActive = false

# Step 4: Verify not in list
GET /api/users
# Deleted user should not appear (filtered out by DeletedAt IS NULL)
```

## Common HTTP Status Codes

| Status Code | Meaning | When |
|-------------|---------|------|
| 200 OK | Success | GET, PUT, DELETE successful |
| 201 Created | Resource created | POST successful |
| 400 Bad Request | Validation error | Invalid input data |
| 404 Not Found | Resource not found | GET/PUT/DELETE non-existent user |
| 409 Conflict | Conflict | Email already exists |
| 500 Internal Server Error | Server error | Unexpected error |
