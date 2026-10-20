import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import CartIndicator from './CartIndicator';
import { CartContext } from 'entities/cart/model/CartContext';

describe('CartIndicator', () => {
    const mockNavigate = jest.fn();

    const renderWithContext = (cartCount, navigate = mockNavigate) => {
        return render(
            <CartContext.Provider value={{ cartCount }}>
                <CartIndicator navigate={navigate} />
            </CartContext.Provider>
        );
    };

    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит кнопку корзины', () => {
        renderWithContext(0);
        expect(screen.getByRole('button')).toBeInTheDocument();
    });

    test('рендерит SVG корзины', () => {
        const { container } = renderWithContext(0);
        expect(container.querySelector('svg')).toBeInTheDocument();
    });

    test('показывает количество товаров', () => {
        renderWithContext(5);
        expect(screen.getByText('5')).toBeInTheDocument();
    });

    test('показывает 0 если товаров нет', () => {
        renderWithContext(0);
        expect(screen.getByText('0')).toBeInTheDocument();
    });

    test('вызывает navigate("/cart") при клике', () => {
        renderWithContext(3);
        fireEvent.click(screen.getByRole('button'));
        expect(mockNavigate).toHaveBeenCalledWith('/cart');
    });

    test('работает если navigate не передан — использует window.location', () => {
        delete window.location;
        window.location = { href: '' };

        render(
            <CartContext.Provider value={{ cartCount: 1 }}>
                <CartIndicator />
            </CartContext.Provider>
        );

        fireEvent.click(screen.getByRole('button'));
        expect(window.location.href).toBe('/cart');
    });

    test('работает без CartContext (cartCount = 0)', () => {
        render(<CartIndicator navigate={mockNavigate} />);
        expect(screen.getByText('0')).toBeInTheDocument();
    });
});