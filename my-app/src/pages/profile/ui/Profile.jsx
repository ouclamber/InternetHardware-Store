import React, { useState, useEffect, useCallback } from 'react';
import './Profile.css';
import { useNavigate } from 'react-router-dom';
import Header from 'widgets/header/ui/Header';
import { userStorage } from 'entities/user/lib/userStorage';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

const Profile = () => {
    const navigate = useNavigate();
    const [userStats, setUserStats] = useState(null);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [loading, setLoading] = useState(true);

    const userId = userStorage.getUserId();
    const userName = userStorage.getUserName() || 'Пользователь';
    const userRole = userStorage.getUserRole() || 'User';
    const token = userStorage.getToken();

    const fetchUserStats = useCallback(async () => {
        try {
            if (!userId) return;

            let basketCount = 0;
            let purchaseCount = 0;
            let totalSpent = 0;

            try {
                const cartResponse = await http(Configuration.Cart.Get);
                const cartData = await cartResponse.json();
                basketCount = pick(cartData, 'TotalQuantity', 'totalQuantity') || 0;
            } catch (e) {
                console.warn('Ошибка загрузки корзины:', e.message);
            }

            try {
                const ordersResponse = await http(Configuration.Orders.GetMy);
                const orders = await ordersResponse.json();
                if (Array.isArray(orders)) {
                    purchaseCount = orders.length;
                    totalSpent = orders.reduce((sum, order) => {
                        const amount = pick(order, 'TotalAmount', 'totalAmount') || 0;
                        return sum + (typeof amount === 'number' ? amount : 0);
                    }, 0);
                }
            } catch (e) {
                console.warn('Ошибка загрузки заказов:', e.message);
            }

            setUserStats({
                basketCount,
                purchaseCount,
                reviewCount: 0,
                totalSpent
            });
        } catch (err) {
            console.error('Ошибка статистики:', err);
            setUserStats({ basketCount: 0, purchaseCount: 0, reviewCount: 0, totalSpent: 0 });
        }
    }, [userId]);

    const refreshAllData = useCallback(async () => {
        setLoading(true);
        await fetchUserStats();
        setLoading(false);
    }, [fetchUserStats]);

    useEffect(() => {
        if (!userId && !token) {
            navigate('/SignIn');
            return;
        }

        localStorage.removeItem('needRefreshProfile');

        refreshAllData();

        const handleVisibilityChange = () => {
            if (!document.hidden) refreshAllData();
        };
        const handleFocus = () => refreshAllData();

        document.addEventListener('visibilitychange', handleVisibilityChange);
        window.addEventListener('focus', handleFocus);
        return () => {
            document.removeEventListener('visibilitychange', handleVisibilityChange);
            window.removeEventListener('focus', handleFocus);
        };
    }, [navigate, userId, token, refreshAllData]);

    const handleLogout = () => {
        userStorage.clear();
        navigate('/SignIn');
    };

    const handlePasswordChange = async () => {
        const oldPassword = prompt('Введите текущий пароль:');
        const newPassword = prompt('Введите новый пароль:');
        const confirmPassword = prompt('Подтвердите новый пароль:');

        if (!oldPassword || !newPassword || !confirmPassword) {
            setError('Все поля обязательны');
            return;
        }
        if (newPassword !== confirmPassword) {
            setError('Пароли не совпадают');
            return;
        }
        if (newPassword.length < 6) {
            setError('Минимум 6 символов');
            return;
        }

        try {
            const response = await http.post(Configuration.Auth.ChangePassword, {
                oldPassword,
                newPassword
            });

            if (response.ok) {
                setSuccess('Пароль изменён. Перенаправление...');
                userStorage.clear();
                setTimeout(() => navigate('/SignIn'), 2000);
            } else {
                const result = await response.json().catch(() => ({}));
                setError(pick(result, 'message', 'Message') || 'Ошибка смены пароля');
            }
        } catch (err) {
            setError('Ошибка смены пароля');
        }
    };

    const displayUserName = userName;
    const displayRole = userRole === 'Admin' ? 'Администратор' : 'Пользователь';

    if (loading) {
        return (
            <div className="loading-spinner">
                <div className="spinner"></div>
                <p>Загрузка профиля...</p>
            </div>
        );
    }

    return (
        <div className="profile-page">
            <Header navigate={navigate} />
            <div className="container">
                <div className="profile-header">
                    <div className="profile-header-left">
                        <h1 className="profile-title">Мой профиль</h1>
                        <button className="btn-back" onClick={() => navigate('/HomePage')}>Назад</button>
                    </div>
                    <div className="profile-actions">
                        {userRole === 'Admin' && (
                            <button className="btn-admin" onClick={() => navigate('/Admin')}>Админ панель</button>
                        )}
                        <button className="btn-password" onClick={handlePasswordChange}>Сменить пароль</button>
                        <button className="btn-logout" onClick={handleLogout}>Выйти</button>
                    </div>
                </div>

                {error && (
                    <div className="alert alert-error">
                        <p>{error}</p>
                        <button onClick={() => setError('')}>×</button>
                    </div>
                )}
                {success && (
                    <div className="alert alert-success">
                        <p>{success}</p>
                        <button onClick={() => setSuccess('')}>×</button>
                    </div>
                )}

                <div className="profile-content">
                    <div className="profile-info-column">
                        <div className="profile-card">
                            <div className="user-header">
                                <div className="user-initials">
                                    {(displayUserName[0] || 'П').toUpperCase()}
                                </div>
                                <div className="user-basic-info">
                                    <h2>{displayUserName}</h2>
                                    <p className="username">@{displayUserName}</p>
                                    <p className="user-role">{displayRole}</p>
                                </div>
                            </div>
                            <div className="profile-form">
                                <div className="form-group">
                                    <label>Имя пользователя</label>
                                    <p className="form-value">{displayUserName}</p>
                                </div>
                                <div className="form-group">
                                    <label>Роль</label>
                                    <p className="form-value">{userRole}</p>
                                </div>
                                <div className="form-group">
                                    <label>ID пользователя</label>
                                    <p className="form-value">{userId}</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="profile-stats-column">
                        <div className="stats-card">
                            <h3 className="stats-title">Статистика</h3>
                            <div className="stats-grid">
                                <div className="stat-item">
                                    <div className="stat-value">{userStats?.basketCount || 0}</div>
                                    <div className="stat-label">Товаров в корзине</div>
                                </div>
                                <div className="stat-item">
                                    <div className="stat-value">{userStats?.purchaseCount || 0}</div>
                                    <div className="stat-label">Заказов</div>
                                </div>
                                <div className="stat-item">
                                    <div className="stat-value">{userStats?.reviewCount || 0}</div>
                                    <div className="stat-label">Отзывов</div>
                                </div>
                                <div className="stat-item">
                                    <div className="stat-value">
                                        {userStats?.totalSpent
                                            ? `${userStats.totalSpent.toLocaleString('ru-RU')} ₽`
                                            : '0 ₽'}
                                    </div>
                                    <div className="stat-label">Потрачено</div>
                                </div>
                            </div>
                        </div>

                        <div className="basket-card">
                            <h3 className="basket-title">Корзина</h3>
                            <div className="basket-stats">
                                <p className="basket-count">
                                    Товаров в корзине: <strong>{userStats?.basketCount || 0}</strong>
                                </p>
                            </div>
                            <button className="btn-view-basket" onClick={() => navigate('/cart')}>
                                Перейти в корзину
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Profile;