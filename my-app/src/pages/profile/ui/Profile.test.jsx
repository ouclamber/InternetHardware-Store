import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import Profile from './Profile';

const mockNavigate = jest.fn();

jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate
}));

jest.mock('widgets/header/ui/Header', () => () => <div data-testid="header" />);

jest.mock('entities/user/lib/userStorage', () => ({
    userStorage: {
        getUserId: () => '1',
        getUserName: () => 'ouclamber',
        getUserRole: () => 'Admin',
        getToken: () => 'fake-token',
        getCreatedAt: () => '2026-10-07T02:13:51',
        clear: jest.fn()
    }
}));

const mockHttp = jest.fn();

jest.mock('shared/api/httpClient', () => ({
    http: (...args) => mockHttp(...args)
}));

describe('Profile', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();

        mockHttp.mockImplementation((url) => {
            if (url.includes('/Cart')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ TotalQuantity: 0, Items: [] })
                });
            }
            if (url.includes('/Orders')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => []
                });
            }
            return Promise.resolve({ ok: true, json: async () => ({}) });
        });
    });

    test('рендерит заголовок "Мой профиль"', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Мой профиль')).toBeInTheDocument();
        });
    });

    test('показывает имя пользователя', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getAllByText('ouclamber').length).toBeGreaterThan(0);
        });
    });

    test('показывает роль Admin', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Администратор')).toBeInTheDocument();
        });
    });

    test('показывает кнопку "Сменить пароль"', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Сменить пароль')).toBeInTheDocument();
        });
    });

    test('показывает кнопку "Выйти"', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Выйти')).toBeInTheDocument();
        });
    });

    test('показывает кнопку "Админ панель" для админа', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Админ панель')).toBeInTheDocument();
        });
    });

    test('клик "Админ панель" → navigate("/Admin")', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Админ панель')).toBeInTheDocument();
        });
        fireEvent.click(screen.getByText('Админ панель'));
        expect(mockNavigate).toHaveBeenCalledWith('/Admin');
    });

    test('клик "Назад" → navigate("/HomePage")', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Назад')).toBeInTheDocument();
        });
        fireEvent.click(screen.getByText('Назад'));
        expect(mockNavigate).toHaveBeenCalledWith('/HomePage');
    });

    test('клик "Выйти" → userStorage.clear + navigate("/SignIn")', async () => {
        const { userStorage } = require('entities/user/lib/userStorage');

        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Выйти')).toBeInTheDocument();
        });

        fireEvent.click(screen.getByText('Выйти'));

        expect(userStorage.clear).toHaveBeenCalled();
        expect(mockNavigate).toHaveBeenCalledWith('/SignIn');
    });

    test('клик "Перейти в корзину" → navigate("/cart")', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Перейти в корзину')).toBeInTheDocument();
        });
        fireEvent.click(screen.getByText('Перейти в корзину'));
        expect(mockNavigate).toHaveBeenCalledWith('/cart');
    });

    test('показывает блок "Статистика"', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Статистика')).toBeInTheDocument();
        });
    });

    test('показывает блок "Корзина"', async () => {
        render(<Profile />);
        await waitFor(() => {
            expect(screen.getByText('Корзина')).toBeInTheDocument();
        });
    });

    // === PASSWORD CHANGE ===

    test('клик "Сменить пароль" вызывает prompt', async () => {
        const originalPrompt = window.prompt;
        window.prompt = jest.fn(() => null);

        render(<Profile />);
        await waitFor(() => expect(screen.getByText('Сменить пароль')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Сменить пароль'));
        expect(window.prompt).toHaveBeenCalled();

        window.prompt = originalPrompt;
    });

    test('смена пароля с пустыми полями показывает ошибку', async () => {
        const originalPrompt = window.prompt;
        window.prompt = jest.fn(() => '');

        render(<Profile />);
        await waitFor(() => expect(screen.getByText('Сменить пароль')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Сменить пароль'));

        await waitFor(() => {
            expect(screen.getByText('Все поля обязательны')).toBeInTheDocument();
        });

        window.prompt = originalPrompt;
    });

    test('смена пароля с несовпадающими паролями показывает ошибку', async () => {
        const originalPrompt = window.prompt;
        let callCount = 0;
        window.prompt = jest.fn(() => {
            callCount++;
            if (callCount === 1) return 'old';
            if (callCount === 2) return 'newpass123';
            return 'different';
        });

        render(<Profile />);
        await waitFor(() => expect(screen.getByText('Сменить пароль')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Сменить пароль'));

        await waitFor(() => {
            expect(screen.getByText('Пароли не совпадают')).toBeInTheDocument();
        });

        window.prompt = originalPrompt;
    });

    test('смена пароля с коротким паролем показывает ошибку', async () => {
        const originalPrompt = window.prompt;
        let callCount = 0;
        window.prompt = jest.fn(() => {
            callCount++;
            if (callCount === 1) return 'old';
            return '123';
        });

        render(<Profile />);
        await waitFor(() => expect(screen.getByText('Сменить пароль')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Сменить пароль'));

        await waitFor(() => {
            expect(screen.getByText('Минимум 6 символов')).toBeInTheDocument();
        });

        window.prompt = originalPrompt;
    });

    // === STATS ===

    test('показывает статистику корзины', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/Cart')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ TotalQuantity: 5, Items: [] })
                });
            }
            if (url.includes('/Orders')) {
                return Promise.resolve({ ok: true, json: async () => [] });
            }
            return Promise.resolve({ ok: true, json: async () => ({}) });
        });

        render(<Profile />);

        await waitFor(() => {
            const stats = screen.getAllByText('5');
            expect(stats.length).toBeGreaterThan(0);
        });
    });

    test('показывает 0 при пустых данных', async () => {
        render(<Profile />);
        await waitFor(() => {
            const zeros = screen.getAllByText('0');
            expect(zeros.length).toBeGreaterThan(0);
        });
    });

    test('алерт ошибки можно закрыть', async () => {
        const originalPrompt = window.prompt;
        window.prompt = jest.fn(() => '');

        render(<Profile />);
        await waitFor(() => expect(screen.getByText('Сменить пароль')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Сменить пароль'));

        await waitFor(() => {
            expect(screen.getByText('Все поля обязательны')).toBeInTheDocument();
        });

        const closeBtn = screen.getByText('×');
        fireEvent.click(closeBtn);

        await waitFor(() => {
            expect(screen.queryByText('Все поля обязательны')).not.toBeInTheDocument();
        });

        window.prompt = originalPrompt;
    });

    test('рендерит инициалы пользователя', async () => {
        render(<Profile />);
        await waitFor(() => {
            const initials = document.querySelector('.user-initials');
            expect(initials).toHaveTextContent('O');
        });
    });
});