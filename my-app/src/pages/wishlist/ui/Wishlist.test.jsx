import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import Wishlist from './Wishlist';

const mockNavigate = jest.fn();
const mockHttp = jest.fn();
const mockRefresh = jest.fn();

jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => <Component {...props} navigate={mockNavigate} />
}));
jest.mock('shared/api/httpClient', () => ({
    http: (...args) => mockHttp(...args)
}));
jest.mock('widgets/header/ui/Header', () => () => <div data-testid="header" />);
jest.mock('widgets/notification/ui/Notification', () => () => null);
jest.mock('shared/ui/BackButton/BackButton', () => ({ onClick }) => <button onClick={onClick}>Назад</button>);
jest.mock('entities/wishlist/model/WishlistContext', () => ({
    useWishlist: () => ({ refresh: mockRefresh })
}));
jest.mock('features/add-to-cart/model/useAddToCart', () => ({
    useAddToCart: () => ({ addToCart: jest.fn(), notification: { show: false } })
}));
jest.mock('features/toggle-wishlist/ui/WishlistButton', () => ({ productId, onResult }) => (
    <button data-testid={`remove-${productId}`} onClick={() => onResult({ success: true, added: false })}>♥</button>
));

const login = () => {
    localStorage.setItem('token', 't');
    localStorage.setItem('userId', '1');
};

const ITEMS = [
    { ProductId: 1, AddedAt: '2026-10-01', Product: { Id: 1, Name: 'Google Pixel 8', Price: 59999, IsActive: true, StockQuantity: 2 } },
    { ProductId: 2, AddedAt: '2026-10-02', Product: { Id: 2, Name: 'JBL Flip 6', Price: 12999, IsActive: true, StockQuantity: 0 } }
];

describe('Wishlist page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
    });

    test('гостю предлагает войти', () => {
        render(<Wishlist />);

        fireEvent.click(screen.getByText('Войти'));
        expect(mockNavigate).toHaveBeenCalledWith('/SignIn');
        expect(mockHttp).not.toHaveBeenCalled();
    });

    test('показывает товары из избранного', async () => {
        login();
        mockHttp.mockResolvedValueOnce({ json: async () => ITEMS });

        render(<Wishlist />);

        expect(await screen.findByText('Google Pixel 8')).toBeInTheDocument();
        expect(screen.getByText('JBL Flip 6')).toBeInTheDocument();
        expect(screen.getByText('(2)')).toBeInTheDocument();
        expect(mockHttp).toHaveBeenCalledWith('/api/Wishlist');
        expect(screen.getByText('Осталось 2 шт.')).toBeInTheDocument();
        expect(screen.getByText('Нет в наличии')).toBeInTheDocument();
    });

    test('удаление из избранного убирает карточку', async () => {
        login();
        mockHttp.mockResolvedValueOnce({ json: async () => ITEMS });
        render(<Wishlist />);
        await screen.findByText('Google Pixel 8');

        fireEvent.click(screen.getByTestId('remove-1'));

        await waitFor(() => expect(screen.queryByText('Google Pixel 8')).not.toBeInTheDocument());
        expect(mockRefresh).toHaveBeenCalled();
    });

    test('пустое избранное', async () => {
        login();
        mockHttp.mockResolvedValueOnce({ json: async () => [] });

        render(<Wishlist />);

        expect(await screen.findByText('В избранном пока пусто.')).toBeInTheDocument();
        fireEvent.click(screen.getByText('Перейти в каталог'));
        expect(mockNavigate).toHaveBeenCalledWith('/HomePage');
    });

    test('ошибка загрузки и повтор', async () => {
        login();
        mockHttp.mockRejectedValueOnce(new Error('Сервер недоступен'));
        mockHttp.mockResolvedValueOnce({ json: async () => ITEMS });

        render(<Wishlist />);

        expect(await screen.findByText('Сервер недоступен')).toBeInTheDocument();
        fireEvent.click(screen.getByText('Повторить'));
        expect(await screen.findByText('Google Pixel 8')).toBeInTheDocument();
    });

    test('клик по карточке открывает товар', async () => {
        login();
        mockHttp.mockResolvedValueOnce({ json: async () => ITEMS });
        render(<Wishlist />);

        fireEvent.click(await screen.findByText('JBL Flip 6'));

        expect(mockNavigate).toHaveBeenCalledWith('/product/2');
    });
});
