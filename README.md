# Internet Hardware Store

Интернет-магазин электроники: каталог, поиск, корзина, оформление заказов, отзывы с модерацией и админ-панель.
Бэкенд построен по принципам **DDD + CQRS** (ASP.NET Core + MediatR), фронтенд — на React без UI-библиотек (собственные лёгкие компоненты).

> Учебный проект для портфолио. Цель — отработать чистую архитектуру, тестирование и CI/CD на «полноценном» приложении.

<!-- Добавьте скриншоты в папку docs/ и раскомментируйте:
![Главная](docs/home.png)
![Каталог](docs/catalog.png)
![Админ-панель](docs/admin.png)
-->

---

## Возможности

**Покупатель**
- Регистрация и вход (JWT); пароль не короче 8 символов
- Каталог по категориям: телевизоры, ноутбуки, компьютеры, смартфоны, колонки
- Фильтры (цена, «только в наличии»), сортировка и пагинация — на стороне сервера
- Наличие на складе: «В наличии / Осталось N шт. / Нет в наличии», нельзя купить больше остатка
- Избранное (сердечко на карточке и странице товара, отдельная страница)
- Поиск товаров
- Страница товара: фотографии, характеристики, отзывы и рейтинг
- Корзина: добавление, изменение количества, удаление
- Оформление заказа: контакты, адрес, способ доставки и оплаты, комментарий
- История заказов, отмена заказа (товар возвращается на склад)
- Смена пароля в профиле: остальные сессии завершаются, текущая продолжается с новым токеном

**Администратор**
- Статистика магазина
- Управление пользователями (роли, удаление)
- Просмотр всех заказов (постранично, фильтр по статусу) и смена их статуса
- Управление каталогом: товары (полное редактирование, снятие/возврат в продажу), цены, бренды, категории, типы, характеристики, изображения
- Склад: остатки товаров, счётчик «нет в наличии» на дашборде
- Модерация отзывов (одобрить/отклонить; включается `Reviews:RequireModeration`)

---

## Технологии

| Слой | Стек |
|------|------|
| Backend | ASP.NET Core (.NET 10), Entity Framework Core 9, PostgreSQL, MediatR (CQRS), JWT, BCrypt, AES-GCM, Rate Limiting, Serilog, Swagger |
| Frontend | React 19, React Router 7, Fetch API, CSS |
| Тесты | xUnit (бэкенд), Jest + React Testing Library (фронтенд), coverlet |
| DevOps | Docker, Docker Compose, GitHub Actions (CI, публикация образа, релизы) |

---

## Архитектура бэкенда

```
Backend/
├── Backend.Domain          # Ядро: агрегаты (Cart, Order), сущности, value objects
│                           # (Money, Quantity, Rating, Address...), интерфейсы репозиториев
├── Backend.Application     # Use cases: Commands / Queries / Handlers / DTO / Mappers (MediatR)
│                           # Модули: Catalog, Sales, Reviews, Users, Admin
├── Backend.Infrastructure  # EF Core, миграции, репозитории, JWT, хеширование паролей
├── Backend.Tests           # Unit-тесты домена, хендлеров, репозиториев и сервисов
├── Controllers/            # Тонкие REST-контроллеры (только вызывают MediatR)
└── Middleware/             # Единая обработка исключений (DomainException → 400)
```

Зависимости направлены внутрь: `Controllers → Application → Domain ← Infrastructure`.
Бизнес-правила живут в домене (например, `Order` и `Cart` сами проверяют инварианты), а контроллеры не содержат логики.

Фронтенд организован по методологии **Feature-Sliced Design**:
`app → pages → widgets → features → entities → shared`.

---

## Быстрый старт

### Требования
- [Docker](https://www.docker.com/) и Docker Compose
- [Node.js](https://nodejs.org/) 20+ (для фронтенда)

### 1. Бэкенд и база данных (Docker)

```bash
git clone <repository-url>
cd InternetHardware-Store

# Секреты хранятся в .env (он не коммитится)
cp .env.example .env
```

Откройте `.env` и заполните значения (команды для генерации секретов есть в самом файле):

```env
POSTGRES_PASSWORD=придумайте-пароль
JWT_SECRET_KEY=<openssl rand -base64 48>
ENCRYPTION_KEY=<openssl rand -base64 32>
ADMIN_USERNAME=admin
ADMIN_PASSWORD=<пароль не короче 8 символов>
```

Запуск:

```bash
docker compose up --build
```

Без обязательных переменных `docker compose` не запустится — это сделано намеренно, чтобы приложение не работало с угадываемыми секретами.

После запуска:

| Сервис | Адрес |
|--------|-------|
| API | http://localhost:5214 |
| Swagger (только Development) | http://localhost:5214/swagger |
| Health-check (жив ли процесс) | http://localhost:5214/health/live |
| Health-check (готов ли — есть БД) | http://localhost:5214/health/ready |
| PostgreSQL | localhost:5433 |

Миграции применяются автоматически, база заполняется тестовыми данными (бренды, категории, товары), а при заданных `ADMIN_USERNAME` и `ADMIN_PASSWORD` создаётся администратор.

### 2. Фронтенд

```bash
cd my-app
npm install
npm start
```

Приложение откроется на http://localhost:3000.
Адрес API по умолчанию — `http://localhost:5214`; изменить его можно переменной `REACT_APP_API_URL`.

### Запуск бэкенда без Docker

Поднимите только базу и задайте секреты через [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (они хранятся вне репозитория):

```bash
docker compose up -d postgres

cd Backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=NEW_DB;Username=postgres;Password=<POSTGRES_PASSWORD>"
dotnet user-secrets set "Jwt:SecretKey" "$(openssl rand -base64 48)"
dotnet user-secrets set "Encryption:Key" "$(openssl rand -base64 32)"
dotnet user-secrets set "Admin:UserName" "admin"
dotnet user-secrets set "Admin:Password" "<пароль не короче 8 символов>"

dotnet run
```

### Администратор

Через регистрацию в приложении (и через API) создаётся только обычный пользователь: роль от клиента не принимается.
Первый администратор создаётся при старте из конфигурации (`Admin:UserName` и `Admin:Password`, в Docker — `ADMIN_USERNAME` и `ADMIN_PASSWORD`), если в базе ещё нет ни одного администратора. Затем он может менять роли пользователей в админ-панели.

---

## Конфигурация

Параметры задаются через `appsettings.json` или переменные окружения (двойное подчёркивание вместо `:`, например `Jwt__SecretKey`).
Секреты в репозитории **не хранятся**: без них приложение не запустится.

| Параметр | Обязателен | Описание |
|----------|:---------:|----------|
| `ConnectionStrings:DefaultConnection` | да | Строка подключения к PostgreSQL |
| `Jwt:SecretKey` | да | Секрет подписи JWT, не короче 32 символов |
| `Encryption:Key` | да | Ключ AES-256: 32 байта в Base64 |
| `Admin:UserName`, `Admin:Password` | нет | Первый администратор (пароль не короче 8 символов) |
| `Jwt:Issuer`, `Jwt:Audience` | нет | Издатель и аудитория токена |
| `Jwt:ExpiresMinutes` | нет | Срок жизни токена, по умолчанию 1440 (сутки) |
| `Cors:AllowedOrigins` | нет | Разрешённые адреса фронтенда (по умолчанию localhost:3000) |
| `RateLimiting:AuthPermitLimit` | нет | Запросов к `/api/Auth/*` в минуту с одного IP, по умолчанию 10 |
| `RateLimiting:GlobalPermitLimit` | нет | Запросов ко всему API в минуту с одного IP, по умолчанию 600 |
| `Reviews:RequireModeration` | нет | `true` — новые и изменённые отзывы публикуются только после одобрения |
| `Seed:Enabled` | нет | Заполнять каталог тестовыми данными вне Development |

Генерация секретов:

```bash
openssl rand -base64 48   # Jwt:SecretKey
openssl rand -base64 32   # Encryption:Key
```

---

## API

Полная интерактивная документация — в Swagger (`/swagger`). Основные группы эндпоинтов:

| Группа | Маршруты | Доступ |
|--------|----------|--------|
| Auth | `POST /api/Auth/signup`, `signin`, `change-password` (лимит запросов на IP) | публично / авторизованный |
| Products | `GET /api/Products`, `/{id}`, `/category/{id}`, `/search?q=` | публично |
| Products (каталог) | `GET /api/Products/paged?categoryId=&brandIds=&minPrice=&maxPrice=&search=&inStockOnly=&sort=&page=&pageSize=` | публично |
| Products (управление) | `POST`, `PUT /{id}`, `PUT /{id}/price`, `PUT /{id}/stock`, `POST /{id}/activate`, `DELETE /{id}` | Admin |
| Wishlist | `GET /api/Wishlist`, `GET /ids`, `PUT /{productId}`, `DELETE /{productId}` | авторизованный |
| Brands, Categories, Types | `GET` публично; `POST`, `PUT`, `DELETE` | Admin |
| ProductAttributes, ProductImages | чтение публично; изменение | Admin |
| Reviews | `GET /product/{id}`; `POST`, `PUT`, `DELETE`; `GET /pending`, `PUT /{id}/approve`, `PUT /{id}/reject` | авторизованный / Admin |
| Cart | `GET`, `POST /items`, `PUT /items/{productId}`, `DELETE` | авторизованный |
| Orders | `GET`, `GET /{id}`, `POST`, `DELETE /{id}`; `PUT /{id}/status` | авторизованный / Admin |
| Admin | `GET /stats`, `/users`, `/orders?status=&page=&pageSize=`; `PUT /users/{id}/role`; `DELETE /users/{id}` | Admin |
| Health | `GET /health/live`, `/health/ready` | публично |

Авторизация: заголовок `Authorization: Bearer <token>`. Токен содержит метку безопасности пользователя:
после смены пароля, смены роли или удаления пользователя ранее выданные токены перестают приниматься.

Ошибки возвращаются как `{ "error": "...", "type": "...", "status": 400 }`:

| Статус | Когда |
|--------|-------|
| 400 | нарушено бизнес-правило (`DomainError`) |
| 403 | нет прав на объект (`AccessDenied`) |
| 404 | объект не найден (`NotFound`) |
| 409 | конфликт: товар раскупили параллельно, дубль уникального значения (`Conflict`) |
| 429 | превышен лимит запросов |

---

## Тесты

```bash
# Бэкенд (из корня репозитория)
dotnet test Backend/Backend.Tests/Backend.Tests.csproj

# С покрытием
dotnet test Backend/Backend.Tests/Backend.Tests.csproj \
  --collect:"XPlat Code Coverage" --settings Backend/coverlet.runsettings

# Фронтенд
cd my-app
npm test
```

В бэкенде более 800 тестов: домен (value objects, агрегаты), хендлеры, репозитории (in-memory БД, в том числе
регрессионные тесты «один запрос — один DbContext»), сервисы, мапперы и конфигурации EF.
На фронтенде — более 540 тестов компонентов, страниц, хуков и API-слоя.

После изменений модели проверьте, что снимок миграций синхронизирован (миграция должна получиться пустой).
`dotnet ef` создаёт контекст через приложение, поэтому нужны те же секреты, что и для запуска
(строка подключения, `Jwt:SecretKey`, `Encryption:Key`):

```bash
cd Backend
dotnet ef migrations add CheckModelSync --project Backend.Infrastructure --startup-project .
dotnet ef migrations remove --project Backend.Infrastructure --startup-project .
```

---

## CI/CD

Workflows в `.github/workflows/`:

| Файл | Назначение |
|------|------------|
| `ci-backend.yml` | Сборка, тесты и отчёт о покрытии бэкенда |
| `ci-frontend.yml` | Сборка и тесты фронтенда |
| `docker-publish.yml` | Публикация Docker-образа бэкенда |
| `cd-release.yml` | Создание GitHub Release по тегу `v*.*.*` |

---

## Планы по развитию

- [x] Проверка остатков на складе при оформлении заказа
- [x] Пагинация и фильтры каталога на стороне API
- [x] Отзыв JWT при смене пароля/роли (метка безопасности)
- [ ] Валидация запросов через pipeline-behavior (FluentValidation)
- [ ] Refresh-токены и более короткий срок жизни access-токена
- [ ] Интеграционные тесты API на реальном PostgreSQL (Testcontainers)
- [ ] Контейнер для фронтенда и единый запуск через `docker compose`
- [ ] Миграция фронтенда с Create React App на Vite

---

## Автор

**Ваше имя** — [GitHub](https://github.com/your-username) · [LinkedIn](https://linkedin.com/in/your-profile)

## Лицензия

MIT
