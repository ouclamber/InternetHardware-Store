import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import SignUp from './SignUp';
 
const mockNavigate = jest.fn();
 
// eslint-disable-next-line no-var
var mockSignUp = jest.fn();
 
jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => (
        <Component {...props} navigate={mockNavigate} />
    )
}));
 
jest.mock('shared/api/AuthServices', () => {
    return jest.fn().mockImplementation(() => ({
        SignIn: jest.fn(),
        SignUp: (...args) => mockSignUp(...args),
        ChangePassword: jest.fn()
    }));
});
 
jest.mock('entities/user/lib/userStorage', () => ({
    userStorage: {
        clear: jest.fn()
    }
}));
 
describe('SignUp', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
    });
 
    test('рендерит заголовок SignUp', () => {
        render(<SignUp />);
        expect(screen.getByText('SignUp')).toBeInTheDocument();
    });
 
    test('рендерит поля UserName, Password, Confirm Password', () => {
        render(<SignUp />);
        expect(screen.getByLabelText('UserName')).toBeInTheDocument();
        expect(screen.getByLabelText('Password')).toBeInTheDocument();
        expect(screen.getByLabelText('Confirm Password')).toBeInTheDocument();
    });
 
    test('не предлагает выбор роли (регистрация всегда создаёт обычного пользователя)', () => {
        render(<SignUp />);
        expect(screen.queryByLabelText('Admin')).not.toBeInTheDocument();
        expect(screen.queryByRole('radio')).not.toBeInTheDocument();
    });
 
    test('заполнение полей обновляет state', () => {
        render(<SignUp />);
        fireEvent.change(screen.getByLabelText('UserName'), { target: { value: 'newuser' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'password123' } });
        fireEvent.change(screen.getByLabelText('Confirm Password'), { target: { value: 'password123' } });
 
        expect(screen.getByLabelText('UserName')).toHaveValue('newuser');
        expect(screen.getByLabelText('Password')).toHaveValue('password123');
        expect(screen.getByLabelText('Confirm Password')).toHaveValue('password123');
    });
 
    test('ошибка при пустом UserName', async () => {
        render(<SignUp />);
        fireEvent.click(screen.getByText('Sign Up'));
 
        await waitFor(() => {
            expect(screen.getByText('Введите имя пользователя')).toBeInTheDocument();
        });
    });
 
    test('ошибка при пустом Password', async () => {
        render(<SignUp />);
        fireEvent.change(screen.getByLabelText('UserName'), { target: { value: 'user' } });
        fireEvent.click(screen.getByText('Sign Up'));
 
        await waitFor(() => {
            expect(screen.getByText('Введите пароль')).toBeInTheDocument();
        });
    });
 
    test('ошибка при коротком пароле (< 8)', async () => {
        render(<SignUp />);
        fireEvent.change(screen.getByLabelText('UserName'), { target: { value: 'user' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: '123' } });
        fireEvent.change(screen.getByLabelText('Confirm Password'), { target: { value: '123' } });
        fireEvent.click(screen.getByText('Sign Up'));
 
        await waitFor(() => {
            expect(screen.getByText('Пароль должен быть минимум 8 символов')).toBeInTheDocument();
        });
    });
 
    test('ошибка при несовпадающих паролях', async () => {
        render(<SignUp />);
        fireEvent.change(screen.getByLabelText('UserName'), { target: { value: 'user' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'password123' } });
        fireEvent.change(screen.getByLabelText('Confirm Password'), { target: { value: 'other12345' } });
        fireEvent.click(screen.getByText('Sign Up'));
 
        await waitFor(() => {
            expect(screen.getByText('Пароли не совпадают')).toBeInTheDocument();
        });
    });
 
    test('клик "Sign In" → navigate("/SignIn")', () => {
        render(<SignUp />);
        fireEvent.click(screen.getByText('Sign In'));
        expect(mockNavigate).toHaveBeenCalledWith('/SignIn');
    });
 
    test('успешная регистрация вызывает SignUp', async () => {
        mockSignUp.mockResolvedValueOnce({ status: 200 });
 
        render(<SignUp />);
        fireEvent.change(screen.getByLabelText('UserName'), { target: { value: 'newuser' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'password123' } });
        fireEvent.change(screen.getByLabelText('Confirm Password'), { target: { value: 'password123' } });
        fireEvent.click(screen.getByText('Sign Up'));
 
        await waitFor(() => {
            expect(mockSignUp).toHaveBeenCalledWith({
                UserName: 'newuser',
                Password: 'password123',
                ConfirmPassword: 'password123'
            });
        });
    });
 
    test('показывает ошибку "Ошибка регистрации" при неверных данных', async () => {
        mockSignUp.mockResolvedValueOnce({
            status: 400,
            data: { message: 'Ошибка регистрации' }
        });
 
        render(<SignUp />);
        fireEvent.change(screen.getByLabelText('UserName'), { target: { value: 'user' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'password123' } });
        fireEvent.change(screen.getByLabelText('Confirm Password'), { target: { value: 'password123' } });
        fireEvent.click(screen.getByText('Sign Up'));
 
        await waitFor(() => {
            expect(screen.getByText('Ошибка регистрации')).toBeInTheDocument();
        });
    });
 
    test('показывает ошибку 409 — "уже существует"', async () => {
        mockSignUp.mockRejectedValueOnce({
            response: { status: 409, data: {} }
        });
 
        render(<SignUp />);
        fireEvent.change(screen.getByLabelText('UserName'), { target: { value: 'user' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'password123' } });
        fireEvent.change(screen.getByLabelText('Confirm Password'), { target: { value: 'password123' } });
        fireEvent.click(screen.getByText('Sign Up'));
 
        await waitFor(() => {
            expect(screen.getByText(/уже существует/)).toBeInTheDocument();
        });
    });
});