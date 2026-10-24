# HardWare Store

Интернет-магазин электроники: каталог с фильтрами, поиск, избранное, корзина, оформление заказа, склад,
отзывы с модерацией и админ-панель.

**Бэкенд** — ASP.NET Core (.NET 10) по принципам **DDD + CQRS** (MediatR), PostgreSQL, EF Core.
**Фронтенд** — React 19 по методологии **Feature-Sliced Design**, без UI-библиотек.

> Учебный проект для портфолио: чистая архитектура, тесты (≈1450), CI/CD и Docker на «настоящем» приложении.

## Содержание

- [Возможности](#возможности)
- [Технологии](#технологии)
- [Быстрый старт](#быстрый-старт)
- [Конфигурация](#конфигурация)
- [Структура проекта](#структура-проекта)
- [API](#api)
- [Производительность](#производительность)
- [Тесты](#тесты)
- [CI/CD](#cicd)
- [Частые проблемы](#частые-проблемы)

---

## Возможности

### Покупатель

- Регистрация и вход по JWT
- Каталог по категориям: телевизоры, ноутбуки, компьютеры, смартфоны, колонки
- Фильтры по цене и наличию, сортировка и пагинация — на стороне сервера
- Поиск по названию и описанию
- Карточка товара: фото, характеристики, отзывы с рейтингом и распределением оценок
- Наличие на складе: «В наличии», «Осталось N шт.», «Нет в наличии»; купить больше остатка нельзя
- Избранное: сердечко на карточке и странице товара, отдельная страница, счётчик в шапке
- Корзина и оформление заказа в несколько шагов: контакты, доставка, оплата, подтверждение
- История заказов и отмена заказа (товар возвращается на склад)
- Профиль: статистика, смена пароля (остальные сессии завершаются)

### Администратор

- Дашборд: пользователи, товары, заказы, выручка, «нет в наличии», отзывы на модерации
- Пользователи: смена роли, удаление (нельзя изменить себя и понизить последнего администратора)
- Заказы постранично с фильтром по статусу, смена статуса
- Каталог: товары, цены, остатки, снятие и возврат в продажу, бренды, категории, типы, характеристики, изображения
- Модерация отзывов (включается параметром `Reviews:RequireModeration`)

### Интерфейс

- Единая дизайн-система на CSS-переменных: фирменный синий `#007bff`, мягкие тени, шрифт Inter
- Анимации: каскадное появление карточек, 3D-наклон категорий за курсором, блики, «удар сердца» в избранном,
  уведомления с таймером. Все они на `transform`/`opacity` и отключаются системной настройкой
  «уменьшить движение» (`prefers-reduced-motion`)
- Адаптивная вёрстка для телефона, планшета и десктопа

---

## Технологии

| Слой | Стек |
|------|------|
| Backend | .NET 10, ASP.NET Core, EF Core 9 + Npgsql, MediatR, JWT, BCrypt, AES-GCM, Output Cache, Rate Limiting, Serilog, Swagger |
| База данных | PostgreSQL 16 (+ `pg_trgm` для поиска) |
| Frontend | React 19, React Router 7, Fetch API, CSS (Create React App + react-app-rewired) |
| Тесты | xUnit, Moq, FluentAssertions, EF Core InMemory, coverlet; Jest + React Testing Library |
| DevOps | Docker, Docker Compose, GitHub Actions |

---

## Быстрый старт

### Требования

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (с Docker Compose)
- [Node.js](https://nodejs.org/) 20+ — для фронтенда
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — только для запуска тестов или бэкенда без Docker

### 1. Секреты

Секреты хранятся в файле `.env` в корне проекта (он не коммитится). Без них `docker compose` намеренно не запустится.

<table>
<tr><th>Windows (PowerShell)</th><th>Linux / macOS</th></tr>
<tr><td>

```powershell
Copy-Item .env.example .env

# сгенерировать ключи
[Convert]::ToBase64String((1..48 | % { Get-Random -Max 256 }) -as [byte[]])
[Convert]::ToBase64String((1..32 | % { Get-Random -Max 256 }) -as [byte[]])
```

</td><td>

```bash
cp .env.example .env

# сгенерировать ключи
openssl rand -base64 48
openssl rand -base64 32
```

</td></tr>
</table>

Заполните `.env`:

```env
POSTGRES_PASSWORD=придумайте-пароль
JWT_SECRET_KEY=<первый ключ, 48 байт>
ENCRYPTION_KEY=<второй ключ, 32 байта>
ADMIN_USERNAME=admin
ADMIN_PASSWORD=Admin12345
```

Пароль администратора — не короче 8 символов, обязательно буква и цифра.

### 2. Бэкенд и база данных

```bash
docker compose up --build -d
```

Проверить, что всё поднялось:

```bash
docker compose ps
docker compose logs -f backend
```

При старте автоматически применяются миграции, каталог заполняется тестовыми товарами (50 шт.) и создаётся
администратор из `.env`.

| Сервис | Адрес |
|--------|-------|
| API | http://localhost:5214 |
| Swagger (в Development) | http://localhost:5214/swagger |
| Проверка «процесс жив» | http://localhost:5214/health/live |
| Проверка «готов, есть БД» | http://localhost:5214/health/ready |
| PostgreSQL (только с этой машины) | localhost:5433 |

### 3. Фронтенд

```bash
cd my-app
npm install
npm start
```

Сайт откроется на http://localhost:3000. Войдите под администратором из `.env` или зарегистрируйте пользователя.

Адрес API по умолчанию — `http://localhost:5214`, изменить его можно переменной `REACT_APP_API_URL`.
Продакшен-сборка: `npm run build` (результат в `my-app/build`).

### Полезные команды Docker

```bash
docker compose down              # остановить
docker compose down -v           # остановить и удалить базу (данные пропадут)
docker compose up --build -d     # пересобрать после изменений бэкенда
```

### Бэкенд без Docker

Поднимите только базу, а секреты задайте через
[user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) — они хранятся вне репозитория:

```bash
docker compose up -d postgres

cd Backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=NEW_DB;Username=postgres;Password=<POSTGRES_PASSWORD>"
dotnet user-secrets set "Jwt:SecretKey" "<ключ 48 байт>"
dotnet user-secrets set "Encryption:Key" "<ключ 32 байта>"
dotnet user-secrets set "Admin:UserName" "admin"
dotnet user-secrets set "Admin:Password" "Admin12345"

dotnet run
```

### Как появляется администратор

Регистрация (в приложении и через API) создаёт только обычного пользователя — роль от клиента не принимается.
Первый администратор создаётся при старте из конфигурации (`Admin:UserName` / `Admin:Password`,
в Docker — `ADMIN_USERNAME` / `ADMIN_PASSWORD`), если в базе ещё нет ни одного администратора.
Дальше роли меняются в админ-панели.

---

## Конфигурация

Параметры задаются в `appsettings.json` или переменными окружения (двойное подчёркивание вместо двоеточия:
`Jwt__SecretKey`). Секреты в репозитории не хранятся.

| Параметр | Обязателен | Описание |
|----------|:---:|----------|
| `ConnectionStrings:DefaultConnection` | да | Строка подключения к PostgreSQL |
| `Jwt:SecretKey` | да | Секрет подписи JWT, не короче 32 символов |
| `Encryption:Key` | да | Ключ AES-256 (32 байта в Base64) — шифрует контакты покупателя в заказах |
| `Admin:UserName`, `Admin:Password` | нет | Первый администратор |
| `Jwt:Issuer`, `Jwt:Audience` | нет | Издатель и аудитория токена |
| `Jwt:ExpiresMinutes` | нет | Срок жизни токена, по умолчанию 1440 (сутки) |
| `Cors:AllowedOrigins` | нет | Адреса фронтенда, по умолчанию `http://localhost:3000` |
| `RateLimiting:AuthPermitLimit` | нет | Запросов к `/api/Auth/*` в минуту с IP, по умолчанию 10 |
| `RateLimiting:GlobalPermitLimit` | нет | Запросов ко всему API в минуту с IP, по умолчанию 600 |
| `Reviews:RequireModeration` | нет | `true` — отзывы публикуются после одобрения администратором |
| `Seed:Enabled` | нет | Заполнять каталог тестовыми данными вне Development |

Соответствие переменных `.env` для Docker: `POSTGRES_PASSWORD`, `JWT_SECRET_KEY`, `ENCRYPTION_KEY`,
`ADMIN_USERNAME`, `ADMIN_PASSWORD`, `ASPNETCORE_ENVIRONMENT`, `SEED_ENABLED`,
`REVIEWS_REQUIRE_MODERATION`, `FRONTEND_ORIGIN` — см. `.env.example`.

---

## Структура проекта

```
├── Backend/
│   ├── Backend.Domain/          # Ядро: агрегаты (Product, Cart, Order, Review, User), value objects
│   │                            # (Money, Quantity, Rating, Address, CustomerContact), интерфейсы репозиториев
│   ├── Backend.Application/     # Сценарии: Commands / Queries / Handlers / DTO / Mappers (MediatR)
│   ├── Backend.Infrastructure/  # EF Core, миграции, репозитории, JWT, BCrypt, шифрование
│   ├── Backend.Tests/           # Тесты домена, хендлеров, репозиториев, сервисов
│   ├── Controllers/             # Тонкие REST-контроллеры — только вызывают MediatR
│   ├── Middleware/              # Обработка исключений, заголовки безопасности
│   ├── Caching/                 # Политика кеширования ответов каталога
│   └── wwwroot/images/          # Изображения товаров
├── my-app/                      # Фронтенд (Feature-Sliced Design)
│   └── src/
│       ├── app/                 # Точка входа, роутинг, провайдеры, глобальные стили
│       ├── pages/               # Страницы: home, catalog/*, product-details, cart, checkout, profile, admin…
│       ├── widgets/             # Шапка, каталог категории, уведомления
│       ├── features/            # Добавление в корзину, избранное
│       ├── entities/            # Товар, категория, корзина, избранное, пользователь
│       └── shared/              # API-клиент, конфиг, утилиты, UI-кит (Form, Pagination, LoadingSpinner…)
├── docker-compose.yml
├── .env.example
└── CHANGELOG.md
```

Зависимости бэкенда направлены внутрь: `Controllers → Application → Domain ← Infrastructure`.
Бизнес-правила живут в домене (например, `Order` и `Cart` сами проверяют инварианты, `Product` — остаток),
контроллеры не содержат логики.

---

## API

Интерактивная документация — Swagger (`/swagger`, в режиме Development).

| Группа | Маршруты | Доступ |
|--------|----------|--------|
| Auth | `POST /api/Auth/signup`, `/signin`, `/change-password` | публично / вошедший |
| Товары | `GET /api/Products`, `/{id}`, `/category/{id}`, `/search?q=` | публично |
| Каталог | `GET /api/Products/paged?categoryId=&typeId=&brandIds=&minPrice=&maxPrice=&search=&inStockOnly=&sort=&page=&pageSize=` | публично |
| Управление товарами | `POST`, `PUT /{id}`, `PUT /{id}/price`, `PUT /{id}/stock`, `POST /{id}/activate`, `DELETE /{id}` | Admin |
| Справочники | `Brands`, `Categories` (с `ProductCount`), `Types`: `GET` публично; `POST`, `PUT`, `DELETE` | Admin |
| Характеристики, изображения | `ProductAttributes`, `ProductImages`: чтение публично, изменение | Admin |
| Избранное | `GET /api/Wishlist`, `GET /ids`, `PUT /{productId}`, `DELETE /{productId}` | вошедший |
| Корзина | `GET /api/Cart`, `POST /items`, `PUT /items/{productId}`, `DELETE /items/{productId}`, `DELETE` | вошедший |
| Заказы | `GET`, `GET /{id}`, `POST`, `DELETE /{id}` (отмена); `PUT /{id}/status` | вошедший / Admin |
| Отзывы | `GET /product/{id}`; `POST`, `PUT /{id}`, `DELETE /{id}`; `GET /pending`, `PUT /{id}/approve`, `/reject` | вошедший / Admin |
| Админ | `GET /api/Admin/stats`, `/users`, `/orders?status=&page=&pageSize=`; `PUT /users/{id}/role`; `DELETE /users/{id}` | Admin |
| Здоровье | `GET /health/live`, `/health/ready` | публично |

Сортировка каталога (`sort`): `price_asc`, `price_desc`, `name_asc`, `name_desc`, `newest`.

**Авторизация** — заголовок `Authorization: Bearer <token>`. В токене хранится метка безопасности
пользователя: после смены пароля, роли или удаления пользователя выданные ранее токены не принимаются.

**Ошибки** возвращаются как `{ "error": "...", "type": "...", "status": 400 }`:

| Статус | Когда |
|--------|-------|
| 400 | нарушено бизнес-правило (`DomainError`) |
| 403 | нет прав (`AccessDenied`) |
| 404 | не найдено (`NotFound`) |
| 409 | конфликт: товар раскупили параллельно, дубль уникального значения (`Conflict`) |
| 429 | превышен лимит запросов |

---

## Производительность

**Бэкенд**

- Пагинация, фильтры и сортировка каталога выполняются в БД; составные индексы под запросы каталога
- Поиск `ILIKE '%…%'` по триграммным GIN-индексам (`pg_trgm`) вместо чтения всей таблицы
- Output cache 30 с для публичных справочников, списков, карточки и поиска; сбрасывается при любом
  изменении каталога или остатков
- Счётчики товаров по категориям — одним `GROUP BY`
- Чтение без отслеживания изменений, split-запросы для вложенных коллекций (без cartesian explosion)
- Сжатие ответов (Brotli/Gzip), кеширование изображений в браузере на неделю

**Фронтенд**

- Страницы загружаются лениво (code splitting по маршрутам)
- Без тяжёлых UI-библиотек: собственный лёгкий UI-кит, весь JS — около 490 КБ (130 КБ gzip)
- Независимые запросы выполняются параллельно; GET без лишнего CORS-preflight
- Ленивая загрузка изображений в списках, приоритетная — для главного фото товара
- Анимации на `transform`/`opacity` (без перерисовок), 3D-наклон обновляется не чаще раза за кадр

---

## Тесты

| | Команда | Количество |
|---|---------|:---:|
| Бэкенд | `dotnet test Backend/Backend.Tests/Backend.Tests.csproj` | 900+ |
| Фронтенд | `cd my-app && npm test -- --watchAll=false` | 545 |

Покрытие бэкенда:

```bash
dotnet test Backend/Backend.Tests/Backend.Tests.csproj --collect:"XPlat Code Coverage" --settings Backend/coverlet.runsettings
```

Покрытие фронтенда: `npm test -- --watchAll=false --coverage`.

Бэкенд: домен (value objects, агрегаты), хендлеры, репозитории на in-memory БД (включая регрессионные
тесты «один запрос — один DbContext»), сервисы, мапперы, конфигурации EF, проверка, что все изображения
из тестовых данных существуют. Фронтенд: компоненты, страницы, хуки, контексты и API-клиент.

### Миграции

После изменения модели проверьте, что снимок миграций синхронизирован — новая миграция должна получиться пустой
(нужны те же секреты, что и для запуска):

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
| `ci-backend.yml` | Сборка, тесты и покрытие бэкенда |
| `ci-frontend.yml` | Сборка и тесты фронтенда |
| `docker-publish.yml` | Публикация Docker-образа бэкенда |
| `cd-release.yml` | GitHub Release по тегу `v*.*.*` |

---

## Частые проблемы

**`docker compose` не запускается: «Задайте POSTGRES_PASSWORD…»**
Нет файла `.env` или в нём пустые обязательные переменные — см. [Секреты](#1-секреты).

**`The container name "/ihs-postgres" is already in use`**
Остались контейнеры от предыдущего запуска (например, из папки другой версии проекта):

```bash
docker rm -f ihs-postgres ihs-backend
docker compose up -d
```

**Бэкенд не подключается к базе после смены `POSTGRES_PASSWORD`**
Пароль задаётся только при первом создании базы. Пересоздайте её (данные пропадут): `docker compose down -v`.

**Ошибка `MSB1009: файл проекта не существует` при `dotnet test`**
Путь указан относительно другой папки. Из корня проекта: `dotnet test Backend/Backend.Tests/Backend.Tests.csproj`,
из папки `Backend`: `dotnet test Backend.Tests/Backend.Tests.csproj`.

**Не отображаются изображения товаров**
Пересоберите бэкенд (`docker compose up --build -d`) — изображения входят в образ. Имена файлов
в `Backend/wwwroot/images` должны быть латиницей: кириллица при упаковке в zip может превратиться в `#U041e…`.

**Ошибка 500**
Подробности — в логах: `docker compose logs backend --tail 100`. В режиме Development текст ошибки есть и в теле ответа.

---

## Автор

**Ваше имя** — [GitHub](https://github.com/your-username) · [LinkedIn](https://linkedin.com/in/your-profile)

## Лицензия

MIT
