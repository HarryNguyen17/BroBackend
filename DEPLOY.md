# Hướng dẫn Deploy Backend lên Server

## Cách 1: Sử dụng Environment Variables (Khuyến nghị cho Production)

ASP.NET Core tự động đọc từ Environment Variables. Không cần file .env trên server.

### Trên Linux/Ubuntu Server:

```bash
# Set environment variables
export ConnectionStrings__DefaultConnection="Host=localhost;Database=grabsimulator;Username=postgres;Password=yourpassword"
export Jwt__Key="your-256-bit-secret-key-here"
export Jwt__Issuer="GrabSimulator"
export Jwt__Audience="GrabSimulatorClient"
export Jwt__ExpiryInDays="30"
export Smtp__Host="smtp.gmail.com"
export Smtp__Port="587"
export Smtp__Username="your-email@gmail.com"
export Smtp__Password="your-app-password"
export Smtp__FromEmail="your-email@gmail.com"
export Smtp__FromName="Grab Simulator"
export ASPNETCORE_ENVIRONMENT="Production"
```

### Hoặc tạo file systemd service với environment variables:

Tạo file `/etc/systemd/system/grabsimulator.service`:

```ini
[Unit]
Description=Grab Simulator API
After=network.target postgresql.service

[Service]
Type=notify
User=www-data
WorkingDirectory=/var/www/grabsimulator
ExecStart=/usr/bin/dotnet /var/www/grabsimulator/Application.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=grabsimulator

# Environment Variables
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ConnectionStrings__DefaultConnection=Host=localhost;Database=grabsimulator;Username=postgres;Password=yourpassword
Environment=Jwt__Key=your-256-bit-secret-key-here
Environment=Jwt__Issuer=GrabSimulator
Environment=Jwt__Audience=GrabSimulatorClient
Environment=Jwt__ExpiryInDays=30
Environment=Smtp__Host=smtp.gmail.com
Environment=Smtp__Port=587
Environment=Smtp__Username=your-email@gmail.com
Environment=Smtp__Password=your-app-password
Environment=Smtp__FromEmail=your-email@gmail.com
Environment=Smtp__FromName=Grab Simulator

[Install]
WantedBy=multi-user.target
```

Sau đó:
```bash
sudo systemctl daemon-reload
sudo systemctl enable grabsimulator
sudo systemctl start grabsimulator
```

### Trên Windows Server (IIS):

1. Mở IIS Manager
2. Chọn Application Pool của bạn
3. Click "Environment Variables" hoặc dùng PowerShell:

```powershell
# Set environment variables cho Application Pool
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Host=localhost;Database=grabsimulator;Username=postgres;Password=yourpassword", "Machine")
[System.Environment]::SetEnvironmentVariable("Jwt__Key", "your-256-bit-secret-key-here", "Machine")
# ... các biến khác
```

### Trên Docker:

Tạo file `docker-compose.yml`:

```yaml
version: '3.8'
services:
  api:
    image: grabsimulator-api:latest
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Database=grabsimulator;Username=postgres;Password=yourpassword
      - Jwt__Key=your-256-bit-secret-key-here
      - Jwt__Issuer=GrabSimulator
      - Jwt__Audience=GrabSimulatorClient
      - Jwt__ExpiryInDays=30
      - Smtp__Host=smtp.gmail.com
      - Smtp__Port=587
      - Smtp__Username=your-email@gmail.com
      - Smtp__Password=your-app-password
      - Smtp__FromEmail=your-email@gmail.com
      - Smtp__FromName=Grab Simulator
    depends_on:
      - db
  
  db:
    image: postgres:15
    environment:
      - POSTGRES_DB=grabsimulator
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=yourpassword
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

## Cách 2: Sử dụng file .env (Cho Local Development)

1. Copy file `.env.example` thành `.env`:
```bash
cp .env.example .env
```

2. Điền thông tin vào file `.env`

3. File `.env` sẽ tự động được load khi chạy ứng dụng (chỉ trong development)

**Lưu ý**: File `.env` đã được thêm vào `.gitignore`, không commit lên Git.

## Cách 3: Sử dụng appsettings.Production.json

1. Tạo file `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=grabsimulator;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "your-production-secret-key",
    "Issuer": "GrabSimulator",
    "Audience": "GrabSimulatorClient",
    "ExpiryInDays": 30
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Grab Simulator"
  }
}
```

2. Set `ASPNETCORE_ENVIRONMENT=Production` trên server

## Thứ tự ưu tiên Configuration (từ cao đến thấp):

1. **Environment Variables** (cao nhất - override tất cả)
2. `appsettings.{Environment}.json` (VD: `appsettings.Production.json`)
3. `appsettings.json` (thấp nhất - default values)

## Bảo mật:

- ✅ **KHÔNG** commit file `.env` hoặc `appsettings.Production.json` có thông tin thật
- ✅ Sử dụng **Environment Variables** trên production server
- ✅ Sử dụng **Secret Management** của cloud provider (Azure Key Vault, AWS Secrets Manager, etc.)
- ✅ JWT Key phải tối thiểu 32 ký tự, khuyến nghị 64+ ký tự
- ✅ Database password phải mạnh
- ✅ Sử dụng HTTPS trong production

## Checklist trước khi Deploy:

- [ ] PostgreSQL đã được cài đặt và chạy trên server
- [ ] Database đã được tạo
- [ ] Migration đã chạy: `dotnet ef database update`
- [ ] Environment Variables đã được set trên server
- [ ] JWT Key đã được generate và set
- [ ] SMTP credentials đã được cấu hình
- [ ] Firewall đã mở port cho API (thường là 80, 443, hoặc 5000)
- [ ] HTTPS/SSL certificate đã được cấu hình (nếu cần)
- [ ] CORS đã được cấu hình đúng (nếu Unity client ở domain khác)

