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
        if (url === Configuration.Admin.Orders) return Promise.resolve(mockJsonResponse(orders));
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

        const selects = screen.getAllByRole('combobox');
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

        screen.getAllByRole('combobox').forEach(s => expect(s).toBeDisabled());
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
});