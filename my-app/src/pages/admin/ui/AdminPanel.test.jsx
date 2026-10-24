import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import AdminPanel from './Admin'; // ← фактическое имя файла (Admin.jsx)
import { CartContext } from 'entities/cart/model/CartContext';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';

const mockNavigate = jest.fn();

jest.mock('shared/api/httpClient', () => ({
    http: Object.assign(
        jest.fn(),
        {
            get: jest.fn(),
            post: jest.fn(),
            put: jest.fn(),
            delete: jest.fn(),
        }
    ),
}));

jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => (
        <Component {...props} navigate={mockNavigate} />
    ),
}));

jest.mock('widgets/header/ui/Header', () => () => <div data-testid="header" />);
jest.mock('shared/ui/LoadingSpinner/LoadingSpinner', () => ({ text }) => (
    <div data-testid="loading-spinner">{text}</div>
));

const createMockContext = (overrides = {}) => ({
    cartItems: [],
    cartCount: 0,
    loading: false,
    addToCart: jest.fn(),
    updateCartItem: jest.fn(),
    removeFromCart: jest.fn(),
    clearCart: jest.fn().mockResolvedValue({ success: true }),
    updateCartCount: jest.fn().mockResolvedValue(),
    refreshCart: jest.fn().mockResolvedValue(),
    ...overrides,
});

const renderAdmin = (ctx = createMockContext()) =>
    render(
        <CartContext.Provider value={ctx}>
            <AdminPanel navigate={mockNavigate} />
        </CartContext.Provider>
    );

const mockJsonResponse = (data) => ({
    ok: true,
    status: 200,
    json: jest.fn().mockResolvedValue(data),
});

const sampleStats = {
    TotalUsers: 10,
    TotalProducts: 25,
    TotalOrders: 5,
    TotalRevenue: 150000,
};

const sampleOrders = [
    { Id: 1, OrderNumber: 'ORD-001', UserName: 'Иван', TotalAmount: 5000, Status: 'pending', CreatedAt: '2025-01-01T10:00:00Z' },
    { Id: 2, OrderNumber: 'ORD-002', UserName: 'Мария', TotalAmount: 3000, Status: 'paid', CreatedAt: '2025-01-02T10:00:00Z' },
];

const sampleUsers = [
    { Id: 1, UserName: 'admin', Role: 'Admin', CreatedAt: '2024-01-01T10:00:00Z' },
    { Id: 2, UserName: 'user1', Role: 'User', CreatedAt: '2024-02-01T10:00:00Z' },
];

// Настраиваем http под каждый сценарий: http(url) — GET, http.put/delete — как в Cart.test.jsx
const setupHttp = ({ stats = sampleStats, orders = sampleOrders, users = sampleUsers } = {}) => {
    http.mockImplementation((url) => {
        if (url === Configuration.Admin.Stats) return Promise.resolve(mockJsonResponse(stats));
        if (url.startsWith(Configuration.Admin.Orders)) return Promise.resolve(mockJsonResponse(orders));
        if (url === Configuration.Admin.Users) return Promise.resolve(mockJsonResponse(users));
        return Promise.reject(new Error(`Unexpected GET: ${url}`));
    });
    http.put.mockResolvedValue(mockJsonResponse({ success: true }));
    http.delete.mockResolvedValue(mockJsonResponse({ success: true }));
};

describe('AdminPanel', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
        localStorage.setItem('userRole', 'Admin');
        window.alert = jest.fn();
        window.confirm = jest.fn(() => true);
        window.scrollTo = jest.fn();
        jest.spyOn(console, 'error').mockImplementation(() => {});
        jest.spyOn(console, 'log').mockImplementation(() => {});
        setupHttp();
    });

    afterEach(() => {
        jest.restoreAllMocks();
    });

    // ---------- Доступ ----------

    test('не-админ: показывает alert и редиректит на /HomePage', async () => {
        localStorage.setItem('userRole', 'User');
        renderAdmin();
        expect(window.alert).toHaveBeenCalledWith(
            'Доступ запрещён. Только для администраторов.'
        );
        expect(mockNavigate).toHaveBeenCalledWith('/HomePage');
        expect(http).not.toHaveBeenCalled();
    });

    test('админ: рендерит заголовок "Админ панель"', async () => {
        renderAdmin();
        expect(await screen.findByText('Админ панель')).toBeInTheDocument();
    });

    test('админ: показывает loading-spinner до загрузки', () => {
        renderAdmin();
        expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();
    });

    // ---------- Дашборд ----------

    test('дашборд: показывает 4 статистики', async () => {
        renderAdmin();
        await screen.findByText('Админ панель');
        expect(screen.getByText('10')).toBeInTheDocument();
        expect(screen.getByText('25')).toBeInTheDocument();
        expect(screen.getByText('5')).toBeInTheDocument();
        expect(screen.getByText('150 000 ₽')).toBeInTheDocument();
    });

    test('дашборд: рендерит блок "Последние заказы"', async () => {
        renderAdmin();
        expect(await screen.findByText('Последние заказы')).toBeInTheDocument();
    });

    test('дашборд: ORD-002 (более новый) идёт первым', async () => {
        renderAdmin();
        await screen.findByText('Последние заказы');
        const rows = screen.getAllByRole('row');
        expect(rows[1]).toHaveTextContent('ORD-002');
        expect(rows[2]).toHaveTextContent('ORD-001');
    });

    test('дашборд: при ошибке загрузки показывает "Ошибка загрузки статистики"', async () => {
        http.mockRejectedValueOnce(new Error('Network'));
        renderAdmin();
        expect(await screen.findByText('Ошибка загрузки статистики')).toBeInTheDocument();
    });

    // ---------- Вкладка "Пользователи" ----------

    test('пользователи: загрузка и рендер', async () => {
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Пользователи' }));
        expect(await screen.findByText('Управление пользователями')).toBeInTheDocument();
        expect(screen.getByText('admin')).toBeInTheDocument();
        expect(screen.getByText('user1')).toBeInTheDocument();
    });

    test('пользователи: для Admin нет кнопки "Удалить"', async () => {
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Пользователи' }));
        await screen.findByText('Управление пользователями');
        const rows = screen.getAllByRole('row');
        expect(rows[1].querySelector('button')).toBeNull(); // admin
        expect(rows[2].querySelector('button')).not.toBeNull(); // user1
    });

    test('пользователи: смена роли вызывает http.put', async () => {
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Пользователи' }));
        await screen.findByText('Управление пользователями');

        const selects = screen.getAllByRole('combobox');
        // второй select — user1 (первый — admin)
        await act(async () => {
            fireEvent.change(selects[1], { target: { value: 'Admin' } });
        });

        expect(http.put).toHaveBeenCalledWith(
            expect.stringContaining('/2/role'),
            expect.objectContaining({ newRole: 'Admin' })
        );
        expect(window.alert).toHaveBeenCalledWith('Роль обновлена');
    });

    test('пользователи: удаление вызывает http.delete', async () => {
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Пользователи' }));
        await screen.findByText('Управление пользователями');

        const rows = screen.getAllByRole('row');
        const userRow = rows[2]; // user1
        const deleteBtn = userRow.querySelector('button');
        await act(async () => {
            fireEvent.click(deleteBtn);
        });

        expect(http.delete).toHaveBeenCalledWith(expect.stringContaining('/2'));
        expect(window.alert).toHaveBeenCalledWith('Пользователь удалён');
    });

    // ---------- Вкладка "Заказы" ----------

    test('заказы: загрузка и рендер', async () => {
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Заказы' }));
        expect(await screen.findByText('Управление заказами')).toBeInTheDocument();
        expect(screen.getByText('Всего заказов: 2')).toBeInTheDocument();
        expect(screen.getByText('ORD-001')).toBeInTheDocument();
        expect(screen.getByText('ORD-002')).toBeInTheDocument();
    });

    test('заказы: смена pending → paid вызывает http.put', async () => {
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Заказы' }));
        await screen.findByText('Управление заказами');

        // Первый combobox — фильтр по статусу; статусы заказов — .status-select
        const selects = document.querySelectorAll('.status-select');
        await act(async () => {
            fireEvent.change(selects[0], { target: { value: 'paid' } });
        });

        expect(http.put).toHaveBeenCalledWith(
            expect.stringContaining('/1/status'),
            expect.objectContaining({ newStatus: 'paid' })
        );
    });

    test('заказы: для delivered/cancelled select disabled', async () => {
        setupHttp({
            orders: [
                { Id: 1, OrderNumber: 'ORD-001', UserName: 'И', TotalAmount: 100, Status: 'delivered', CreatedAt: '2025-01-01' },
                { Id: 2, OrderNumber: 'ORD-002', UserName: 'М', TotalAmount: 200, Status: 'cancelled', CreatedAt: '2025-01-02' },
            ],
        });
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Заказы' }));
        await screen.findByText('Управление заказами');

        document.querySelectorAll('.status-select').forEach(s => expect(s).toBeDisabled());
        expect(screen.getByLabelText('Фильтр по статусу')).not.toBeDisabled();
    });

    test('заказы: не отправляет запрос, если статус не изменился', async () => {
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Заказы' }));
        await screen.findByText('Управление заказами');

        http.put.mockClear();
        const selects = screen.getAllByRole('combobox');
        await act(async () => {
            fireEvent.change(selects[0], { target: { value: 'pending' } });
        });
        expect(http.put).not.toHaveBeenCalled();
    });

    test('клик "Назад" → navigate(-1)', async () => {
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Назад' }));
        expect(mockNavigate).toHaveBeenCalledWith(-1);
    });

    // ===== Новое: пагинация заказов, модерация отзывов, склад =====

    test('parseOrdersResponse понимает постраничный и старый формат', () => {
        const { parseOrdersResponse } = require('./Admin');

        expect(parseOrdersResponse({ Items: [{ Id: 1 }], TotalCount: 41, TotalPages: 3 }))
            .toEqual({ orders: [{ Id: 1 }], totalCount: 41, totalPages: 3 });
        expect(parseOrdersResponse([{ Id: 1 }, { Id: 2 }]))
            .toEqual({ orders: [{ Id: 1 }, { Id: 2 }], totalCount: 2, totalPages: 1 });
        expect(parseOrdersResponse(null)).toEqual({ orders: [], totalCount: 0, totalPages: 0 });
    });

    test('дашборд показывает «Нет в наличии» и «Отзывов на модерации»', async () => {
        setupHttp({ stats: { ...sampleStats, OutOfStockProducts: 4, PendingReviews: 7 } });
        renderAdmin();

        expect(await screen.findByText('Нет в наличии')).toBeInTheDocument();
        expect(screen.getByText('4')).toBeInTheDocument();
        expect(screen.getByText('Отзывов на модерации')).toBeInTheDocument();
        expect(screen.getByText('7')).toBeInTheDocument();
    });

    test('заказы: постраничная загрузка и фильтр по статусу', async () => {
        setupHttp({ orders: { Items: sampleOrders, TotalCount: 45, TotalPages: 3, Page: 1 } });
        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Заказы' }));
        await screen.findByText('Управление заказами');

        expect(screen.getByText('Всего заказов: 45')).toBeInTheDocument();
        expect(http).toHaveBeenCalledWith(`${Configuration.Admin.Orders}?page=1&pageSize=20`);

        await act(async () => {
            fireEvent.click(screen.getByText('2'));
        });
        expect(http).toHaveBeenCalledWith(`${Configuration.Admin.Orders}?page=2&pageSize=20`);

        await act(async () => {
            fireEvent.change(screen.getByLabelText('Фильтр по статусу'), { target: { value: 'paid' } });
        });
        expect(http).toHaveBeenCalledWith(`${Configuration.Admin.Orders}?page=1&pageSize=20&status=paid`);
    });

    test('отзывы: одобрение и отклонение', async () => {
        const pending = [
            { Id: 11, ProductName: 'Pixel 8', UserName: 'anna', Rating: 5, Comment: 'Отличный телефон', CreatedAt: '2026-10-01' },
            { Id: 12, ProductName: 'JBL Flip 6', UserName: 'bob', Rating: 1, Comment: 'Спам спам спам', CreatedAt: '2026-10-02' }
        ];
        setupHttp();
        const base = http.getMockImplementation();
        http.mockImplementation((url) =>
            url === Configuration.Reviews.Pending ? Promise.resolve(mockJsonResponse(pending)) : base(url));
        http.put.mockResolvedValue({ ok: true });

        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Отзывы' }));

        expect(await screen.findByText('Отличный телефон')).toBeInTheDocument();

        await act(async () => {
            fireEvent.click(screen.getAllByText('Одобрить')[0]);
        });
        expect(http.put).toHaveBeenCalledWith('/api/Reviews/11/approve');
        expect(screen.queryByText('Отличный телефон')).not.toBeInTheDocument();

        await act(async () => {
            fireEvent.click(screen.getByText('Отклонить'));
        });
        expect(http.put).toHaveBeenCalledWith('/api/Reviews/12/reject');
        expect(screen.getByText('Нет отзывов, ожидающих модерации')).toBeInTheDocument();
    });

    test('склад: изменение остатка товара', async () => {
        const products = { Items: [{ Id: 5, Name: 'JBL Flip 6', Price: 12999, StockQuantity: 0 }], TotalPages: 1 };
        setupHttp();
        const base = http.getMockImplementation();
        http.mockImplementation((url) =>
            url.startsWith(Configuration.Products.Paged) ? Promise.resolve(mockJsonResponse(products)) : base(url));
        http.put.mockResolvedValue({ ok: true });

        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Склад' }));

        const input = await screen.findByLabelText('Новый остаток: JBL Flip 6');
        expect(http).toHaveBeenCalledWith(`${Configuration.Products.Paged}?page=1&pageSize=20&sort=name_asc`);

        fireEvent.change(input, { target: { value: '15' } });
        await act(async () => {
            fireEvent.click(screen.getByText('Сохранить'));
        });

        expect(http.put).toHaveBeenCalledWith('/api/Products/5/stock', { quantity: 15 });
        expect(screen.getByText('15')).toBeInTheDocument();
    });

    test('склад: некорректный остаток не отправляется', async () => {
        const products = { Items: [{ Id: 5, Name: 'JBL Flip 6', Price: 12999, StockQuantity: 3 }], TotalPages: 1 };
        setupHttp();
        const base = http.getMockImplementation();
        http.mockImplementation((url) =>
            url.startsWith(Configuration.Products.Paged) ? Promise.resolve(mockJsonResponse(products)) : base(url));
        const alertSpy = jest.spyOn(window, 'alert').mockImplementation(() => {});

        renderAdmin();
        await screen.findByText('Админ панель');
        fireEvent.click(screen.getByRole('button', { name: 'Склад' }));

        fireEvent.change(await screen.findByLabelText('Новый остаток: JBL Flip 6'), { target: { value: '-2' } });
        await act(async () => {
            fireEvent.click(screen.getByText('Сохранить'));
        });

        expect(http.put).not.toHaveBeenCalled();
        expect(alertSpy).toHaveBeenCalled();
        alertSpy.mockRestore();
    });
});

