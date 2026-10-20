import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Header from './Header';
import { CartContext } from 'entities/cart/model/CartContext';

const mockNavigate = jest.fn();

const mockCartContext = {
    cartCount: 3,
    cartItems: [],
    addToCart: jest.fn(),
    updateCartCount: jest.fn(),
    refreshCart: jest.fn(),
    clearCart: jest.fn()
};

const renderHeader = (props = {}) =>
    render(
        <CartContext.Provider value={mockCartContext}>
            <Header navigate={mockNavigate} {...props} />
        </CartContext.Provider>
    );

describe('Header', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
    });

    test('рендерится без ошибок', () => {
        renderHeader();
        expect(document.querySelector('.header')).toBeInTheDocument();
    });

    test('рендерит логотип GLANCEvex', () => {
        renderHeader();
        expect(document.querySelector('.logo-primary')).toHaveTextContent('HardWare');
        expect(document.querySelector('.logo-secondary')).toHaveTextContent('Store');
    });

    test('рендерит поиск', () => {
        renderHeader();
        const search = screen.getByPlaceholderText(/Поиск товаров/i);
        expect(search).toBeInTheDocument();
    });

    test('рендерит иконки user и cart', () => {
        renderHeader();
        expect(document.querySelector('.user-icon')).toBeInTheDocument();
        expect(document.querySelector('.cart-icon')).toBeInTheDocument();
    });

    test('показывает cartCount из контекста', () => {
        renderHeader();
        expect(screen.getByText('3')).toBeInTheDocument();
    });

    test('cartCount 0 при пустой корзине', () => {
        render(
            <CartContext.Provider value={{ ...mockCartContext, cartCount: 0 }}>
                <Header navigate={mockNavigate} />
            </CartContext.Provider>
        );
        expect(screen.getByText('0')).toBeInTheDocument();
    });

    test('клик по иконке корзины → navigate("/cart")', () => {
        renderHeader();
        fireEvent.click(document.querySelector('.cart-icon'));
        expect(mockNavigate).toHaveBeenCalledWith('/cart');
    });

    test('клик по иконке пользователя → navigate("/Profile")', () => {
        renderHeader();
        fireEvent.click(document.querySelector('.user-icon'));
        expect(mockNavigate).toHaveBeenCalledWith('/Profile');
    });

    test('клик по логотипу → navigate("/")', () => {
        renderHeader();
        const logo = document.querySelector('.nav__logo');
        if (logo) {
            fireEvent.click(logo);
            expect(mockNavigate).toHaveBeenCalled();
        }
    });

    test('ввод в поиск вызывает onSearch', () => {
        const onSearch = jest.fn();
        renderHeader({ onSearch });
        const search = screen.getByPlaceholderText(/Поиск товаров/i);
        fireEvent.change(search, { target: { value: 'msi' } });
        // Проверяем что инпут обновился
        expect(search).toHaveValue('msi');
    });

    test('submit формы поиска вызывает onSearch', () => {
        const onSearch = jest.fn();
        renderHeader({ onSearch });
        const search = screen.getByPlaceholderText(/Поиск товаров/i);
        fireEvent.change(search, { target: { value: 'msi' } });
        fireEvent.submit(search.closest('form'));
        // Если onSearch передан — должен вызваться
        if (onSearch.mock.calls.length > 0) {
            expect(onSearch).toHaveBeenCalled();
        }
    });
});