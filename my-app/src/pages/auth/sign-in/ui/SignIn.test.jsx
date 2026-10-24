import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import SignIn from './SignIn';

const mockNavigate = jest.fn();
const mockSignIn = jest.fn();

jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => (
        <Component {...props} navigate={mockNavigate} />
    )
}));

// Фабрика ссылается на hoisted переменные (mockSignIn)
jest.mock('shared/api/AuthServices', () => {
    return jest.fn().mockImplementation(() => ({
        SignIn: (...args) => mockSignIn(...args),
        SignUp: jest.fn(),
        ChangePassword: jest.fn()
    }));
});

describe('SignIn', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
    });

    test('рендерит заголовок SignIn', () => {
        render(<SignIn />);
        expect(screen.getByText('SignIn')).toBeInTheDocument();
    });

    test('рендерит поля UserName и Password', () => {
        render(<SignIn />);
        expect(screen.getByLabelText('UserName')).toBeInTheDocument();
        expect(screen.getByLabelText('Password')).toBeInTheDocument();
    });

    test('рендерит radio-кнопки ролей', () => {
        render(<SignIn />);
        expect(screen.getByLabelText('Admin')).toBeInTheDocument();
        expect(screen.getByLabelText('User')).toBeInTheDocument();
    });

    test('по умолчанию выбран User', () => {
        render(<SignIn />);
        expect(screen.getByLabelText('User')).toBeChecked();
    });

    test('переключение на Admin', () => {
        render(<SignIn />);
        fireEvent.click(screen.getByLabelText('Admin'));
        expect(screen.getByLabelText('Admin')).toBeChecked();
    });

    test('ошибка при пустом UserName', async () => {
        render(<SignIn />);
        fireEvent.click(screen.getByText('Sign In'));

        await waitFor(() => {
            expect(screen.getByText('Введите имя пользователя')).toBeInTheDocument();
        });
    });

    test('ошибка при пустом Password', async () => {
        render(<SignIn />);
        fireEvent.change(screen.getByLabelText('UserName'), {
            target: { value: 'user' }
        });
        fireEvent.click(screen.getByText('Sign In'));

        await waitFor(() => {
            expect(screen.getByText('Введите пароль')).toBeInTheDocument();
        });
    });

    test('успешный вход вызывает SignIn', async () => {
        mockSignIn.mockResolvedValueOnce({
            data: {
                isSuccess: true,
                token: 'fake-token',
                userId: 1,
                userName: 'user',
                role: 'User'
            }
        });

        render(<SignIn />);
        fireEvent.change(screen.getByLabelText('UserName'), { target: { value: 'user' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'pass123' } });
        fireEvent.click(screen.getByText('Sign In'));

        await waitFor(() => {
            expect(mockSignIn).toHaveBeenCalledWith({
                UserName: 'user',
                Password: 'pass123',
                Role: 'User'
            });
        });
    });

    test('клик на "Create New Account" → navigate("/SignUp")', () => {
        render(<SignIn />);
        fireEvent.click(screen.getByText('Create New Account'));
        expect(mockNavigate).toHaveBeenCalledWith('/SignUp');
    });

    test('показывает ошибку "Ошибка входа" при неверных данных', async () => {
        mockSignIn.mockResolvedValueOnce({
            data: { isSuccess: false, message: 'Ошибка входа' }
        });

        render(<SignIn />);
        fireEvent.change(screen.getByLabelText('UserName'), { target: { value: 'user' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'wrong' } });
        fireEvent.click(screen.getByText('Sign In'));

        await waitFor(() => {
            expect(screen.getByText('Ошибка входа')).toBeInTheDocument();
        });
    });
});