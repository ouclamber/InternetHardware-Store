import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import Cart from './Cart';
import { CartContext } from 'entities/cart/model/CartContext';
import { http } from 'shared/api/httpClient';
 
const mockNavigate = jest.fn();
 
jest.mock('shared/api/httpClient', () => ({
    http: Object.assign(
        jest.fn(),
        {
            get: jest.fn(),
            post: jest.fn(),
            put: jest.fn(),
            delete: jest.fn()
        }
    )
}));
 
jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => (
        <Component {...props} navigate={mockNavigate} />
    )
}));
 
jest.mock('widgets/header/ui/Header', () => () => <div data-testid="header" />);
jest.mock('shared/ui/BackButton/BackButton', () => ({ onClick }) => (
    <button data-testid="back-btn" onClick={onClick}>Назад</button>
));
 
jest.mock('entities/product/ui/QuantityControl', () => ({
    quantity,
    onIncrease,
    onDecrease,
    disabled
}) => (
    <div data-testid="qty" data-quantity={quantity} data-disabled={disabled}>
        <button data-testid={`qty-inc-${quantity}`} onClick={onIncrease} disabled={disabled}>+</button>
        <button data-testid={`qty-dec-${quantity}`} onClick={onDecrease} disabled={disabled}>-</button>
    </div>
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
    ...overrides
});
 
const renderCart = (ctx) =>
    render(
        <CartContext.Provider value={ctx}>
            <Cart navigate={mockNavigate} />
        </CartContext.Provider>
    );
 
describe('Cart', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        window.confirm = jest.fn(() => true);
    });
 
    test('рендерит заголовок "Корзина"', () => {
        renderCart(createMockContext());
        expect(screen.getByText('Корзина')).toBeInTheDocument();
    });
 
    test('показывает пустую корзину', () => {
        renderCart(createMockContext());
        expect(screen.getByText(/Ваша корзина пуста/i)).toBeInTheDocument();
    });
 
    test('показывает кнопку "Продолжить покупки"', () => {
        renderCart(createMockContext());
        expect(screen.getByText('Продолжить покупки')).toBeInTheDocument();
    });
 
    test('клик "Продолжить покупки" → navigate("/HomePage")', () => {
        renderCart(createMockContext());
        fireEvent.click(screen.getByText('Продолжить покупки'));
        expect(mockNavigate).toHaveBeenCalledWith('/HomePage');
    });
 
    test('рендерит кнопку "Назад"', () => {
        renderCart(createMockContext());
        expect(screen.getByTestId('back-btn')).toBeInTheDocument();
    });
 
    test('клик "Назад" → navigate(-1)', () => {
        renderCart(createMockContext());
        fireEvent.click(screen.getByTestId('back-btn'));
        expect(mockNavigate).toHaveBeenCalledWith(-1);
    });
 
    test('рендерит товары из контекста', () => {
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 2,
                product: { id: 5, name: 'ASUS ROG', price: 129999, images: [] }
            }]
        }));
        expect(screen.getByText('ASUS ROG')).toBeInTheDocument();
    });
 
    test('рендерит кнопку "Очистить корзину" при наличии товаров', () => {
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }]
        }));
        expect(screen.getByText('Очистить корзину')).toBeInTheDocument();
    });
 
    test('рендерит кнопку "Оформить заказ" при наличии товаров', () => {
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }]
        }));
        expect(screen.getByText('Оформить заказ')).toBeInTheDocument();
    });
 
    test('клик "Оформить заказ" → navigate("/checkout")', () => {
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }]
        }));
        fireEvent.click(screen.getByText('Оформить заказ'));
        expect(mockNavigate).toHaveBeenCalledWith(
            '/checkout',
            expect.any(Object)
        );
    });
 
    test('рендерит QuantityControl для каждого товара', () => {
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 2,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }]
        }));
        expect(screen.getAllByTestId('qty').length).toBe(1);
    });
 
    test('summary показывает правильное количество товаров', () => {
        renderCart(createMockContext({
            cartItems: [
                { id: 1, productId: 5, quantity: 2, product: { id: 5, name: 'A', price: 1000, images: [] } },
                { id: 2, productId: 6, quantity: 3, product: { id: 6, name: 'B', price: 500, images: [] } }
            ]
        }));
        expect(screen.getByText(/Товары \(2 шт\.\)/)).toBeInTheDocument();
    });
 
    test('summary показывает правильную сумму', () => {
        renderCart(createMockContext({
            cartItems: [
                { id: 1, productId: 5, quantity: 2, product: { id: 5, name: 'A', price: 1000, images: [] } }
            ]
        }));
        const totals = screen.getAllByText(/2\s?000/);
        expect(totals.length).toBeGreaterThan(0);
    });
 
    test('summary пересчитывается при двух товарах', () => {
        renderCart(createMockContext({
            cartItems: [
                { id: 1, productId: 5, quantity: 2, product: { id: 5, name: 'A', price: 1000, images: [] } },
                { id: 2, productId: 6, quantity: 1, product: { id: 6, name: 'B', price: 500, images: [] } }
            ]
        }));
        const totals = screen.getAllByText(/2\s?500/);
        expect(totals.length).toBeGreaterThan(0);
    });
 
    test('если cartItems пуст — показывает пустую корзину', () => {
        renderCart(createMockContext({ cartItems: [] }));
        expect(screen.getByText(/Ваша корзина пуста/)).toBeInTheDocument();
    });
 
    test('если cartItems заполнен — не показывает пустую корзину', () => {
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }]
        }));
        expect(screen.queryByText(/Ваша корзина пуста/)).not.toBeInTheDocument();
    });
 
    test('рендерит placeholder 📷 без изображений', () => {
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }]
        }));
        expect(screen.getByText('📷')).toBeInTheDocument();
    });
 
    test('рендерит img с абсолютным URL', () => {
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: {
                    id: 5, name: 'Товар', price: 1000,
                    images: [{ imageUrl: 'https://example.com/img.jpg' }]
                }
            }]
        }));
        const img = screen.getByAltText('Товар');
        expect(img).toHaveAttribute('src', 'https://example.com/img.jpg');
    });
 
    test('рендерит img с относительным URL + API_URL', () => {
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: {
                    id: 5, name: 'Товар', price: 1000,
                    images: [{ imageUrl: '/uploads/img.jpg' }]
                }
            }]
        }));
        const img = screen.getByAltText('Товар');
        expect(img.getAttribute('src')).toContain('/uploads/img.jpg');
    });
 
    test('клик + на QuantityControl вызывает http.put', async () => {
        http.put.mockResolvedValueOnce({ ok: true });
 
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 2,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }]
        }));
 
        const incBtn = screen.getByTestId('qty-inc-2');
        await act(async () => {
            fireEvent.click(incBtn);
        });
 
        expect(http.put).toHaveBeenCalledWith(
            expect.stringContaining('/api/Cart/items/5'),
            { quantity: 3 }
        );
    });
 
    test('клик - при quantity=1 вызывает removeItem (http.delete)', async () => {
        http.delete.mockResolvedValueOnce({ ok: true });
 
        renderCart(createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }]
        }));
 
        const decBtn = screen.getByTestId('qty-dec-1');
        await act(async () => {
            fireEvent.click(decBtn);
        });
 
        expect(http.delete).toHaveBeenCalledWith(
            expect.stringContaining('/api/Cart/items/5')
        );
    });
 
    test('клик "Очистить корзину" → window.confirm', async () => {
        const ctx = createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }]
        });
 
        renderCart(ctx);
        await act(async () => {
            fireEvent.click(screen.getByText('Очистить корзину'));
        });
 
        expect(window.confirm).toHaveBeenCalled();
        expect(ctx.clearCart).toHaveBeenCalled();
    });
 
    test('clearCart с success:false показывает error notification', async () => {
        const ctx = createMockContext({
            cartItems: [{
                id: 1, productId: 5, quantity: 1,
                product: { id: 5, name: 'Товар', price: 1000, images: [] }
            }],
            clearCart: jest.fn().mockResolvedValue({ success: false })
        });
 
        renderCart(ctx);
        await act(async () => {
            fireEvent.click(screen.getByText('Очистить корзину'));
        });
 
        expect(ctx.clearCart).toHaveBeenCalled();
    });
});