# Blog.MVC

基于 ASP.NET Core MVC 的个人博客系统，包含前台展示与后台管理；适合单节点部署、撰写 Markdown 文章并管理分类与标签。

## 功能特性

## 技术栈

- .NET 10 / ASP.NET Core MVC
- BeaverX（模块化应用框架）
- Entity Framework Core + MySQL
- Bootstrap 5
- Editor.md + Markdig
- MinIO（对象存储，图片上传）

## 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL 8.x
- MinIO（可选，用于编辑器图片上传；不配置时其余功能仍可使用）

## 快速开始

### 1. 克隆仓库

```bash
git clone <repository-url>
cd Blog.MVC/Blog.MVC
```

### 2. 配置数据库与 MinIO

编辑 `appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Port=3306;Database=blog_mvc;User=root;Password=your_password;"
  },
  "Minio": {
    "Endpoint": "http://127.0.0.1:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "Bucket": "blog",
    "UseSsl": false
  }
}
```

### 3. 还原客户端库

visual studio右键`libman.json`还原客户端库

### 4. 运行

```bash
dotnet run
```

启动时会自动执行 EF 迁移并写入种子数据。

### 5. 访问地址

| 页面 | 地址 |
|------|------|
| 前台首页 | `http://localhost:5xxx/` |
| 后台登录 | `http://localhost:5xxx/Admin/Account/Login` |

默认账号（首次种子数据）：

- 用户名：`admin`
- 密码：`Admin@123`

## 项目结构

```
Blog.MVC/
├── Areas/Admin/          # 后台 Area（文章、分类、标签、账号）
├── Controllers/          # 前台控制器
├── Data/                 # DbContext、迁移、种子数据
├── IServices/            # 服务接口与 DTO
├── Models/               # 实体模型
├── Services/             # 业务服务实现
├── Views/                # 前台 Razor 视图
├── wwwroot/              # 静态资源与样式
└── BlogMVCModule.cs      # 应用模块与启动配置
```

## 数据库迁移

手动添加迁移：

```bash
dotnet ef migrations add <MigrationName> --output-dir Data/Migrations
```

应用会在启动时通过 `BlogDbSeeder` 自动调用 `MigrateAsync()`。

## 许可证

本项目采用 [MIT License](LICENSE) 开源。
