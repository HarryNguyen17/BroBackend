# Grab Simulator Backend API

Backend ASP.NET Core cho game Unity Grab Simulator với các tính năng:
- Đăng nhập qua Email OTP
- Quản lý Coin (Lưu/Load)
- Bảng xếp hạng (Leaderboard)

## Công nghệ sử dụng

- ASP.NET Core 8.0
- PostgreSQL
- JWT Authentication
- Entity Framework Core
- SMTP Email (Gmail)

## Cài đặt và Chạy

### 1. Cài đặt Dependencies

```bash
dotnet restore
```

### 2. Cấu hình Database và Email

**Cách 1: Sử dụng file .env (Local Development)**

1. Copy `env.example.txt` thành `.env`:
```bash
cp env.example.txt .env
```

2. Mở file `.env` và điền thông tin:
   - PostgreSQL connection string
   - JWT secret key (tối thiểu 32 ký tự)
   - SMTP credentials (Gmail App Password)

**Cách 2: Sử dụng appsettings.Development.json**

Chỉnh sửa file `appsettings.Development.json` với thông tin của bạn.

### 3. Tạo Database và Migration

```bash
# Tạo migration
dotnet ef migrations add InitialCreate

# Áp dụng migration vào database
dotnet ef database update
```

### 4. Chạy ứng dụng

```bash
dotnet run
```

API sẽ chạy tại: `https://localhost:7059` hoặc `http://localhost:5284`

Swagger UI: `https://localhost:7059/swagger`

## API Endpoints

### Authentication (Không cần JWT)

- `POST /api/auth/request-otp` - Gửi OTP đến email
- `POST /api/auth/verify-otp` - Xác thực OTP và nhận JWT token

### Coin Management (Cần JWT)

- `GET /api/coins` - Lấy số coin hiện tại
- `PUT /api/coins` - Cập nhật số coin

### Leaderboard (Cần JWT)

- `GET /api/leaderboard?top=100` - Lấy top N người chơi

## Deploy lên Server

Xem file [DEPLOY.md](DEPLOY.md) để biết chi tiết cách deploy.

**Tóm tắt:**
- **Production**: Sử dụng **Environment Variables** (không dùng file .env)
- **Development**: Có thể dùng file `.env` hoặc `appsettings.Development.json`

## Cấu trúc Project

```
Application/
├── Controllers/          # API Controllers
├── Data/                 # DbContext
├── Models/               # Entities và DTOs
├── Services/             # Business Logic Services
├── appsettings.json      # Default configuration
└── Program.cs            # Application entry point
```

## Bảo mật

- ✅ File `.env` đã được thêm vào `.gitignore`
- ✅ Không commit thông tin nhạy cảm lên Git
- ✅ Sử dụng Environment Variables trên production
- ✅ JWT token có thời hạn 30 ngày
- ✅ OTP có thời hạn 5 phút và chỉ dùng 1 lần

## License

MIT

