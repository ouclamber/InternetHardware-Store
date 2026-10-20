import React, { Component } from 'react';
import './Admin.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import { CartContext } from 'entities/cart/model/CartContext';
import Header from 'widgets/header/ui/Header';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
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
            totalOrdersCount: 0
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
            const [statsResponse, ordersResponse] = await Promise.all([
                http(Configuration.Admin.Stats),
                http(Configuration.Admin.Orders)
            ]);

            const stats = await statsResponse.json();
            const ordersData = await ordersResponse.json();

            let orders = [];
            if (Array.isArray(ordersData)) {
                orders = ordersData;
            } else if (ordersData && typeof ordersData === 'object') {
                orders = ordersData.Orders || ordersData.orders || ordersData.items || [];
            }

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
                totalOrdersCount: orders.length,
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

    loadOrders = async () => {
        this.setState({ loading: true, error: null });
        try {
            const response = await http(Configuration.Admin.Orders);
            const data = await response.json();

            let orders = [];
            let totalCount = 0;

            if (Array.isArray(data)) {
                orders = data;
                totalCount = data.length;
            } else if (data && typeof data === 'object') {
                orders = data.Orders || data.orders || data.items || [];
                totalCount = data.TotalCount || data.totalCount || orders.length;
            }

            this.setState({
                orders,
                totalOrdersCount: totalCount,
                loading: false
            });
        } catch (error) {
            console.error('[loadOrders] Ошибка:', error);
            this.setState({ error: 'Ошибка загрузки заказов', loading: false });
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
        else if (tab === 'orders') this.loadOrders();
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
        const { orders, totalOrdersCount } = this.state;

        return (
            <div className="admin-orders">
                <div className="orders-header">
                    <h3>Управление заказами</h3>
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
                        </div>

                        {loading && <LoadingSpinner text="Загрузка..." />}
                        {error && <div className="error-message"><p>{error}</p></div>}

                        {!loading && !error && (
                            <div className="admin-content">
                                {activeTab === 'dashboard' && this.renderDashboard()}
                                {activeTab === 'users' && this.renderUsers()}
                                {activeTab === 'orders' && this.renderOrders()}
                            </div>
                        )}
                    </div>
                </main>
            </div>
        );
    }
}

export default withNavigate(AdminPanel);