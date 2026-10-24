import React, { Component } from 'react';
import './Admin.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import { CartContext } from 'entities/cart/model/CartContext';
import Header from 'widgets/header/ui/Header';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import Pagination from 'shared/ui/Pagination/Pagination';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

export const ORDERS_PAGE_SIZE = 20;
export const STOCK_PAGE_SIZE = 20;

/**
 * Ответ GET /api/Admin/orders — постраничный ({ Items, TotalCount, TotalPages }).
 * Старый формат (массив) тоже поддерживается.
 */
export const parseOrdersResponse = (data) => {
    if (Array.isArray(data)) {
        return { orders: data, totalCount: data.length, totalPages: 1 };
    }
    if (data && typeof data === 'object') {
        const orders = data.Items || data.items || data.Orders || data.orders || [];
        return {
            orders,
            totalCount: data.TotalCount ?? data.totalCount ?? orders.length,
            totalPages: data.TotalPages ?? data.totalPages ?? 1
        };
    }
    return { orders: [], totalCount: 0, totalPages: 0 };
};

const STATUS_LABELS = {
    pending: 'Ожидает оплаты',
    paid: 'Оплачен',
    shipped: 'Отправлен',
    delivered: 'Доставлен',
    cancelled: 'Отменён'
};

class AdminPanel extends Component {
    static contextType = CartContext;

    constructor(props) {
        super(props);
        this.state = {
            activeTab: 'dashboard',
            users: [],
            orders: [],
            stats: null,
            loading: true,
            error: null,
            totalOrdersCount: 0,
            ordersPage: 1,
            ordersTotalPages: 0,
            ordersStatusFilter: '',
            pendingReviews: [],
            stockProducts: [],
            stockPage: 1,
            stockTotalPages: 0,
            stockSearch: '',
            stockDrafts: {}
        };
        this.abortController = null;
    }

    componentDidMount() {
        const userRole = localStorage.getItem('userRole');
        if (userRole !== 'Admin') {
            alert('Доступ запрещён. Только для администраторов.');
            this.props.navigate('/HomePage');
            return;
        }
        this.loadDashboard();
    }

    componentWillUnmount() {
        if (this.abortController) this.abortController.abort();
    }

    loadDashboard = async () => {
        this.setState({ loading: true, error: null });
        try {
            // Для дашборда достаточно первой страницы — сервер сортирует новые сверху
            const [statsResponse, ordersResponse] = await Promise.all([
                http(Configuration.Admin.Stats),
                http(`${Configuration.Admin.Orders}?page=1&pageSize=10`)
            ]);

            const stats = await statsResponse.json();
            const { orders, totalCount } = parseOrdersResponse(await ordersResponse.json());

            const recentOrders = orders
                .slice()
                .sort((a, b) => {
                    const da = new Date(pick(a, 'CreatedAt', 'createdAt') || 0);
                    const db = new Date(pick(b, 'CreatedAt', 'createdAt') || 0);
                    return db - da;
                })
                .slice(0, 10);

            const statsWithOrders = {
                ...stats,
                RecentOrders: recentOrders
            };

            this.setState({
                stats: statsWithOrders,
                orders,
                totalOrdersCount: totalCount,
                loading: false
            });
        } catch (error) {
            console.error('[loadDashboard] Ошибка:', error);
            this.setState({ error: 'Ошибка загрузки статистики', loading: false });
        }
    };

    loadUsers = async () => {
        this.setState({ loading: true, error: null });
        try {
            const response = await http(Configuration.Admin.Users);
            const users = await response.json();
            this.setState({ users: Array.isArray(users) ? users : [], loading: false });
        } catch (error) {
            console.error('[loadUsers] Ошибка:', error);
            this.setState({ error: 'Ошибка загрузки пользователей', loading: false });
        }
    };

    loadOrders = async (page = this.state.ordersPage, status = this.state.ordersStatusFilter) => {
        this.setState({ loading: true, error: null });
        try {
            const params = new URLSearchParams({ page: String(page), pageSize: String(ORDERS_PAGE_SIZE) });
            if (status) params.set('status', status);

            const response = await http(`${Configuration.Admin.Orders}?${params.toString()}`);
            const { orders, totalCount, totalPages } = parseOrdersResponse(await response.json());

            this.setState({
                orders,
                totalOrdersCount: totalCount,
                ordersTotalPages: totalPages,
                ordersPage: page,
                ordersStatusFilter: status,
                loading: false
            });
        } catch (error) {
            console.error('[loadOrders] Ошибка:', error);
            this.setState({ error: 'Ошибка загрузки заказов', loading: false });
        }
    };

    // === Модерация отзывов ===

    loadPendingReviews = async () => {
        this.setState({ loading: true, error: null });
        try {
            const response = await http(Configuration.Reviews.Pending);
            const reviews = await response.json();
            this.setState({ pendingReviews: Array.isArray(reviews) ? reviews : [], loading: false });
        } catch (error) {
            console.error('[loadPendingReviews] Ошибка:', error);
            this.setState({ error: 'Ошибка загрузки отзывов', loading: false });
        }
    };

    moderateReview = async (reviewId, action) => {
        try {
            await http.put(`${Configuration.Reviews.Moderate}/${reviewId}/${action}`);
            this.setState(prev => ({
                pendingReviews: prev.pendingReviews.filter(r => pick(r, 'Id', 'id') !== reviewId)
            }));
        } catch (error) {
            console.error('[moderateReview] Ошибка:', error);
            alert(error.message || 'Ошибка модерации отзыва');
        }
    };

    // === Склад ===

    loadStock = async (page = this.state.stockPage, search = this.state.stockSearch) => {
        this.setState({ loading: true, error: null });
        try {
            const params = new URLSearchParams({ page: String(page), pageSize: String(STOCK_PAGE_SIZE), sort: 'name_asc' });
            if (search && search.trim()) params.set('search', search.trim());

            const response = await http(`${Configuration.Products.Paged}?${params.toString()}`);
            const data = await response.json();

            this.setState({
                stockProducts: pick(data, 'Items', 'items') || [],
                stockTotalPages: pick(data, 'TotalPages', 'totalPages') || 0,
                stockPage: page,
                stockDrafts: {},
                loading: false
            });
        } catch (error) {
            console.error('[loadStock] Ошибка:', error);
            this.setState({ error: 'Ошибка загрузки товаров', loading: false });
        }
    };

    handleStockDraft = (productId, value) => {
        this.setState(prev => ({ stockDrafts: { ...prev.stockDrafts, [productId]: value } }));
    };

    saveStock = async (productId) => {
        const raw = this.state.stockDrafts[productId];
        const quantity = Number(raw);
        if (raw === undefined || raw === '' || !Number.isInteger(quantity) || quantity < 0) {
            alert('Остаток должен быть целым неотрицательным числом');
            return;
        }

        try {
            await http.put(`${Configuration.Products.Stock}/${productId}/stock`, { quantity });
            this.setState(prev => {
                const { [productId]: _, ...drafts } = prev.stockDrafts;
                return {
                    stockDrafts: drafts,
                    stockProducts: prev.stockProducts.map(p =>
                        pick(p, 'Id', 'id') === productId ? { ...p, StockQuantity: quantity, InStock: quantity > 0 } : p)
                };
            });
        } catch (error) {
            console.error('[saveStock] Ошибка:', error);
            alert(error.message || 'Ошибка обновления остатка');
        }
    };

    updateUserRole = async (userId, newRole) => {
        if (!window.confirm(`Изменить роль пользователя на "${newRole}"?`)) return;

        try {
            await http.put(`${Configuration.Admin.UpdateRole}/${userId}/role`, { newRole });
            alert('Роль обновлена');
            await this.loadUsers();
        } catch (error) {
            console.error('[updateUserRole] Ошибка:', error);
            let message = 'Ошибка обновления роли';
            try {
                if (error.response) {
                    const errorData = await error.response.json();
                    message = errorData.error || errorData.message || message;
                }
            } catch (e) { /* ignore */ }
            alert(message);
        }
    };

    updateOrderStatus = async (orderId, newStatus) => {
        if (!newStatus) return;

        const order = this.state.orders.find(o => pick(o, 'Id', 'id') === orderId);
        const currentStatus = order ? pick(order, 'Status', 'status') : null;

        if (currentStatus === newStatus) {
            console.log('Статус не изменился, пропускаем');
            return;
        }

        if (currentStatus === 'delivered' || currentStatus === 'cancelled') {
            alert(`Заказ уже ${currentStatus === 'delivered' ? 'доставлен' : 'отменён'} — изменить нельзя`);
            await this.loadOrders();
            return;
        }

        try {
            await http.put(`${Configuration.Orders.UpdateStatus}/${orderId}/status`, { newStatus });
            alert('Статус обновлён');

            if (this.state.activeTab === 'dashboard') {
                await this.loadDashboard();
            } else {
                await this.loadOrders();
            }
        } catch (error) {
            console.error('[updateOrderStatus] Ошибка:', error);

            let message = 'Ошибка обновления статуса';
            try {
                if (error.response) {
                    const errorData = await error.response.json();
                    message = errorData.error || errorData.message || message;
                }
            } catch (e) { /* ignore */ }

            alert(message);
            if (this.state.activeTab === 'dashboard') {
                await this.loadDashboard();
            } else {
                await this.loadOrders();
            }
        }
    };

    deleteUser = async (userId) => {
        if (!window.confirm('Удалить пользователя?')) return;

        try {
            await http.delete(`${Configuration.Admin.DeleteUser}/${userId}`);
            alert('Пользователь удалён');
            await this.loadUsers();
        } catch (error) {
            console.error('[deleteUser] Ошибка:', error);
            let message = 'Ошибка удаления пользователя';
            try {
                if (error.response) {
                    const errorData = await error.response.json();
                    message = errorData.error || errorData.message || message;
                }
            } catch (e) { /* ignore */ }
            alert(message);
        }
    };

    switchTab = (tab) => {
        this.setState({ activeTab: tab, error: null });
        if (tab === 'users') this.loadUsers();
        else if (tab === 'orders') this.loadOrders(1, this.state.ordersStatusFilter);
        else if (tab === 'reviews') this.loadPendingReviews();
        else if (tab === 'stock') this.loadStock(1, this.state.stockSearch);
        else this.loadDashboard();
    };

    handleGoBack = () => this.props.navigate(-1);

    renderDashboard = () => {
        const { stats } = this.state;
        if (!stats) return <div className="empty-state">Нет данных</div>;

        const totalUsers = pick(stats, 'TotalUsers', 'totalUsers') || 0;
        const totalProducts = pick(stats, 'TotalProducts', 'totalProducts') || 0;
        const totalOrders = pick(stats, 'TotalOrders', 'totalOrders') || 0;
        const totalRevenue = pick(stats, 'TotalRevenue', 'totalRevenue') || 0;
        const recentOrders = pick(stats, 'RecentOrders', 'recentOrders') || [];
        const outOfStock = pick(stats, 'OutOfStockProducts', 'outOfStockProducts') || 0;
        const pendingReviews = pick(stats, 'PendingReviews', 'pendingReviews') || 0;

        return (
            <div className="admin-dashboard">
                <div className="stats-grid">
                    <div className="stat-card">
                        <div className="stat-info">
                            <h3>{totalUsers}</h3>
                            <p>Пользователей</p>
                        </div>
                    </div>
                    <div className="stat-card">
                        <div className="stat-info">
                            <h3>{totalProducts}</h3>
                            <p>Товаров</p>
                        </div>
                    </div>
                    <div className="stat-card">
                        <div className="stat-info">
                            <h3>{totalOrders}</h3>
                            <p>Заказов</p>
                        </div>
                    </div>
                    <div className="stat-card">
                        <div className="stat-info">
                            <h3>{totalRevenue.toLocaleString('ru-RU')} ₽</h3>
                            <p>Выручка</p>
                        </div>
                    </div>
                    <div className={`stat-card ${outOfStock > 0 ? 'stat-card--warning' : ''}`}>
                        <div className="stat-info">
                            <h3>{outOfStock}</h3>
                            <p>Нет в наличии</p>
                        </div>
                    </div>
                    <div className={`stat-card ${pendingReviews > 0 ? 'stat-card--warning' : ''}`}>
                        <div className="stat-info">
                            <h3>{pendingReviews}</h3>
                            <p>Отзывов на модерации</p>
                        </div>
                    </div>
                </div>

                <div className="recent-orders">
                    <h3>Последние заказы</h3>
                    {recentOrders && recentOrders.length > 0 ? (
                        <table className="admin-table">
                            <thead>
                                <tr>
                                    <th>№ заказа</th>
                                    <th>Пользователь</th>
                                    <th>Сумма</th>
                                    <th>Статус</th>
                                    <th>Дата</th>
                                </tr>
                            </thead>
                            <tbody>
                                {recentOrders.map((order, index) => {
                                    const id = pick(order, 'Id', 'id') ?? index;
                                    const orderNumber = pick(order, 'OrderNumber', 'orderNumber') || '—';
                                    const userName = pick(order, 'UserName', 'userName') || '—';
                                    const totalAmount = pick(order, 'TotalAmount', 'totalAmount') || 0;
                                    const status = pick(order, 'Status', 'status') || 'pending';
                                    const createdAt = pick(order, 'CreatedAt', 'createdAt');

                                    return (
                                        <tr key={id}>
                                            <td>{orderNumber}</td>
                                            <td>{userName}</td>
                                            <td>{totalAmount.toLocaleString('ru-RU')} ₽</td>
                                            <td>
                                                <span className={`status-badge status-${status}`}>
                                                    {STATUS_LABELS[status] || status}
                                                </span>
                                            </td>
                                            <td>{createdAt ? new Date(createdAt).toLocaleDateString('ru-RU') : '—'}</td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    ) : (
                        <p>Нет заказов</p>
                    )}
                </div>
            </div>
        );
    };

    renderUsers = () => {
        const { users } = this.state;

        if (!users || users.length === 0) {
            return <div className="empty-state">Нет пользователей</div>;
        }

        return (
            <div className="admin-users">
                <h3>Управление пользователями</h3>
                <table className="admin-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Имя</th>
                            <th>Роль</th>
                            <th>Дата регистрации</th>
                            <th>Действия</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map((user, index) => {
                            const userId = pick(user, 'Id', 'id') ?? index;
                            const userName = pick(user, 'UserName', 'userName') || '—';
                            const currentRole = pick(user, 'Role', 'role') || 'User';
                            const createdAt = pick(user, 'CreatedAt', 'createdAt');

                            return (
                                <tr key={userId}>
                                    <td>{userId}</td>
                                    <td>{userName}</td>
                                    <td>
                                        <select
                                            value={currentRole}
                                            onChange={(e) => this.updateUserRole(userId, e.target.value)}
                                            className="role-select"
                                        >
                                            <option value="User">User</option>
                                            <option value="Admin">Admin</option>
                                        </select>
                                    </td>
                                    <td>
                                        {createdAt
                                            ? new Date(createdAt).toLocaleDateString('ru-RU')
                                            : '—'}
                                    </td>
                                    <td>
                                        {currentRole !== 'Admin' && (
                                            <button
                                                className="delete-btn"
                                                onClick={() => this.deleteUser(userId)}
                                            >
                                                Удалить
                                            </button>
                                        )}
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
            </div>
        );
    };

    renderOrders = () => {
        const { orders, totalOrdersCount, ordersPage, ordersTotalPages, ordersStatusFilter } = this.state;

        return (
            <div className="admin-orders">
                <div className="orders-header">
                    <h3>Управление заказами</h3>
                    <select
                        className="status-filter"
                        aria-label="Фильтр по статусу"
                        value={ordersStatusFilter}
                        onChange={(e) => this.loadOrders(1, e.target.value)}
                    >
                        <option value="">Все статусы</option>
                        {Object.entries(STATUS_LABELS).map(([code, label]) => (
                            <option key={code} value={code}>{label}</option>
                        ))}
                    </select>
                    <span className="orders-total">
                        Всего заказов: {totalOrdersCount || orders.length || 0}
                    </span>
                </div>

                {orders && orders.length > 0 ? (
                    <table className="admin-table">
                        <thead>
                            <tr>
                                <th>№ заказа</th>
                                <th>Пользователь</th>
                                <th>Сумма</th>
                                <th>Статус</th>
                                <th>Дата</th>
                                <th>Действия</th>
                            </tr>
                        </thead>
                        <tbody>
                            {orders.map((order, index) => {
                                const orderId = pick(order, 'Id', 'id') ?? index;
                                const orderNumber = pick(order, 'OrderNumber', 'orderNumber') || '—';
                                const userName = pick(order, 'UserName', 'userName') || '—';
                                const totalAmount = pick(order, 'TotalAmount', 'totalAmount') || 0;
                                const currentStatus = pick(order, 'Status', 'status') || 'pending';
                                const createdAt = pick(order, 'CreatedAt', 'createdAt');

                                const isFinal = currentStatus === 'delivered' || currentStatus === 'cancelled';

                                return (
                                    <tr key={orderId}>
                                        <td>{orderNumber}</td>
                                        <td>{userName}</td>
                                        <td>{totalAmount.toLocaleString('ru-RU')} ₽</td>
                                        <td>
                                            <select
                                                value={currentStatus}
                                                onChange={(e) => this.updateOrderStatus(orderId, e.target.value)}
                                                className="status-select"
                                                disabled={isFinal}
                                            >
                                                <option value={currentStatus} disabled>
                                                    {STATUS_LABELS[currentStatus] || currentStatus}
                                                </option>

                                                {currentStatus === 'pending' && (
                                                    <>
                                                        <option value="paid">Оплатить</option>
                                                        <option value="cancelled">Отменить</option>
                                                    </>
                                                )}
                                                {currentStatus === 'paid' && (
                                                    <>
                                                        <option value="shipped">Отправить</option>
                                                        <option value="cancelled">Отменить</option>
                                                    </>
                                                )}
                                                {currentStatus === 'shipped' && (
                                                    <option value="delivered">Доставить</option>
                                                )}
                                            </select>
                                        </td>
                                        <td>
                                            {createdAt
                                                ? new Date(createdAt).toLocaleDateString('ru-RU')
                                                : '—'}
                                        </td>
                                        <td>
                                            {!isFinal && (
                                                <button
                                                    className="delete-btn"
                                                    onClick={() => {
                                                        if (window.confirm('Отменить заказ?')) {
                                                            this.updateOrderStatus(orderId, 'cancelled');
                                                        }
                                                    }}
                                                >
                                                    Отменить
                                                </button>
                                            )}
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                ) : (
                    <p>Нет заказов</p>
                )}

                <Pagination
                    currentPage={ordersPage}
                    totalPages={ordersTotalPages}
                    onPageChange={(page) => this.loadOrders(page, ordersStatusFilter)}
                />
            </div>
        );
    };

    renderReviews = () => {
        const { pendingReviews } = this.state;

        if (!pendingReviews.length) {
            return <div className="empty-state">Нет отзывов, ожидающих модерации</div>;
        }

        return (
            <div className="admin-reviews">
                <h3>Отзывы на модерации</h3>
                <table className="admin-table">
                    <thead>
                        <tr>
                            <th>Товар</th>
                            <th>Автор</th>
                            <th>Оценка</th>
                            <th>Комментарий</th>
                            <th>Дата</th>
                            <th>Действия</th>
                        </tr>
                    </thead>
                    <tbody>
                        {pendingReviews.map(review => {
                            const id = pick(review, 'Id', 'id');
                            const createdAt = pick(review, 'CreatedAt', 'createdAt');
                            return (
                                <tr key={id}>
                                    <td>{pick(review, 'ProductName', 'productName') || '—'}</td>
                                    <td>{pick(review, 'UserName', 'userName') || '—'}</td>
                                    <td>{'★'.repeat(pick(review, 'Rating', 'rating') || 0)}</td>
                                    <td className="review-comment-cell">{pick(review, 'Comment', 'comment')}</td>
                                    <td>{createdAt ? new Date(createdAt).toLocaleDateString('ru-RU') : '—'}</td>
                                    <td className="actions-cell">
                                        <button className="approve-btn" onClick={() => this.moderateReview(id, 'approve')}>
                                            Одобрить
                                        </button>
                                        <button className="delete-btn" onClick={() => this.moderateReview(id, 'reject')}>
                                            Отклонить
                                        </button>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
            </div>
        );
    };

    renderStock = () => {
        const { stockProducts, stockPage, stockTotalPages, stockSearch, stockDrafts } = this.state;

        return (
            <div className="admin-stock">
                <div className="orders-header">
                    <h3>Остатки на складе</h3>
                    <form
                        className="stock-search"
                        onSubmit={(e) => { e.preventDefault(); this.loadStock(1, stockSearch); }}
                    >
                        <input
                            type="search"
                            placeholder="Поиск товара..."
                            value={stockSearch}
                            onChange={(e) => this.setState({ stockSearch: e.target.value })}
                        />
                        <button type="submit">Найти</button>
                    </form>
                </div>

                {stockProducts.length === 0 ? (
                    <p>Товары не найдены</p>
                ) : (
                    <table className="admin-table">
                        <thead>
                            <tr>
                                <th>Товар</th>
                                <th>Цена</th>
                                <th>Остаток</th>
                                <th>Новый остаток</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {stockProducts.map(product => {
                                const id = pick(product, 'Id', 'id');
                                const stock = pick(product, 'StockQuantity', 'stockQuantity') ?? 0;
                                const draft = stockDrafts[id];
                                return (
                                    <tr key={id} className={stock === 0 ? 'row-out-of-stock' : ''}>
                                        <td>{pick(product, 'Name', 'name')}</td>
                                        <td>{(pick(product, 'Price', 'price') || 0).toLocaleString('ru-RU')} ₽</td>
                                        <td>{stock === 0 ? <span className="status-badge status-cancelled">0</span> : stock}</td>
                                        <td>
                                            <input
                                                type="number"
                                                min="0"
                                                className="stock-input"
                                                aria-label={`Новый остаток: ${pick(product, 'Name', 'name')}`}
                                                value={draft ?? ''}
                                                placeholder={String(stock)}
                                                onChange={(e) => this.handleStockDraft(id, e.target.value)}
                                            />
                                        </td>
                                        <td>
                                            <button
                                                className="approve-btn"
                                                disabled={draft === undefined || draft === ''}
                                                onClick={() => this.saveStock(id)}
                                            >
                                                Сохранить
                                            </button>
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                )}

                <Pagination
                    currentPage={stockPage}
                    totalPages={stockTotalPages}
                    onPageChange={(page) => this.loadStock(page, stockSearch)}
                />
            </div>
        );
    };

    render() {
        const { activeTab, loading, error } = this.state;

        return (
            <div className="admin-panel">
                <Header navigate={this.props.navigate} />

                <main className="main-content">
                    <div className="container admin-container">
                        <div className="admin-header">
                            <h1 className="admin-title">Админ панель</h1>
                            <button className="back-button" onClick={this.handleGoBack}>
                                Назад
                            </button>
                        </div>

                        <div className="admin-tabs">
                            <button
                                className={`tab ${activeTab === 'dashboard' ? 'active' : ''}`}
                                onClick={() => this.switchTab('dashboard')}
                            >
                                Статистика
                            </button>
                            <button
                                className={`tab ${activeTab === 'users' ? 'active' : ''}`}
                                onClick={() => this.switchTab('users')}
                            >
                                Пользователи
                            </button>
                            <button
                                className={`tab ${activeTab === 'orders' ? 'active' : ''}`}
                                onClick={() => this.switchTab('orders')}
                            >
                                Заказы
                            </button>
                            <button
                                className={`tab ${activeTab === 'reviews' ? 'active' : ''}`}
                                onClick={() => this.switchTab('reviews')}
                            >
                                Отзывы
                            </button>
                            <button
                                className={`tab ${activeTab === 'stock' ? 'active' : ''}`}
                                onClick={() => this.switchTab('stock')}
                            >
                                Склад
                            </button>
                        </div>

                        {loading && <LoadingSpinner text="Загрузка..." />}
                        {error && <div className="error-message"><p>{error}</p></div>}

                        {!loading && !error && (
                            <div className="admin-content">
                                {activeTab === 'dashboard' && this.renderDashboard()}
                                {activeTab === 'users' && this.renderUsers()}
                                {activeTab === 'orders' && this.renderOrders()}
                                {activeTab === 'reviews' && this.renderReviews()}
                                {activeTab === 'stock' && this.renderStock()}
                            </div>
                        )}
                    </div>
                </main>
            </div>
        );
    }
}

export default withNavigate(AdminPanel);