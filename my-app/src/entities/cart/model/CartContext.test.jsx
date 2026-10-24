import React from 'react';
import { renderHook, act, waitFor } from '@testing-library/react';
import { CartProvider, CartContext, useCart } from './CartContext';
import { useContext } from 'react';
import { http } from 'shared/api/httpClient';

jest.mock('shared/api/httpClient', () => ({
    http: Object.assign(jest.fn(), {
        get: jest.fn(),
        post: jest.fn(),
        put: jest.fn(),
        delete: jest.fn()
    })
}));

const wrapper = ({ children }) => <CartProvider>{children}</CartProvider>;

describe('CartContext', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
    });

    test('если не авторизован — корзина пуста', async () => {
        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.cartItems).toEqual([]);
        expect(result.current.cartCount).toBe(0);
    });

    test('если авторизован — грузит корзину с API', async () => {
        localStorage.setItem('token', 'fake-token');
        localStorage.setItem('userId', '1');

        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({
                Items: [
                    { Id: 1, ProductId: 5, Quantity: 2, ProductName: 'ASUS ROG', UnitPrice: 100000 }
                ],
                TotalQuantity: 2,
                TotalAmount: 200000
            })
        });

        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.cartItems).toHaveLength(1);
        expect(result.current.cartItems[0].product.name).toBe('ASUS ROG');
        expect(result.current.cartCount).toBe(2);
        expect(result.current.cartTotal).toBe(200000);
    });

    test('addToCart возвращает ошибку, если не авторизован', async () => {
        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => expect(result.current.loading).toBe(false));

        let response;
        await act(async () => {
            response = await result.current.addToCart(5, 1);
        });

        expect(response.success).toBe(false);
        expect(response.message).toContain('войдите');
    });

    test('addToCart вызывает POST и перезагружает корзину', async () => {
        localStorage.setItem('token', 'fake-token');
        localStorage.setItem('userId', '1');

        // 1. Первый вызов http() — loadCart() на старте
        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ Items: [], TotalQuantity: 0 })
        });

        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => expect(result.current.loading).toBe(false));

        // 2. addToCart: http.post → успех
        http.post.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ isSuccess: true, message: 'OK' })
        });

        // 3. loadCart() ПОСЛЕ addToCart: снова http()
        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ Items: [], TotalQuantity: 0 })
        });

        let response;
        await act(async () => {
            response = await result.current.addToCart(5, 2);
        });

        expect(http.post).toHaveBeenCalledWith(
            expect.stringContaining('/api/Cart'),
            { productId: 5, quantity: 2 }
        );
        expect(response.success).toBe(true);
    });

    test('addToCart возвращает success: false при ошибке API', async () => {
        localStorage.setItem('token', 'fake-token');
        localStorage.setItem('userId', '1');

        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ Items: [], TotalQuantity: 0 })
        });

        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => expect(result.current.loading).toBe(false));

        const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
        http.post.mockRejectedValueOnce(new Error('Network error'));

        let response;
        await act(async () => {
            response = await result.current.addToCart(5, 1);
        });

        expect(response.success).toBe(false);
        consoleError.mockRestore();
    });

    test('updateCartItem вызывает PUT', async () => {
        localStorage.setItem('token', 'fake-token');
        localStorage.setItem('userId', '1');

        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ Items: [], TotalQuantity: 0 })
        });

        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => expect(result.current.loading).toBe(false));

        http.put.mockResolvedValueOnce({ ok: true });
        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ Items: [], TotalQuantity: 0 })
        });

        await act(async () => {
            await result.current.updateCartItem(5, 3);
        });

        expect(http.put).toHaveBeenCalledWith(
            expect.stringContaining('/api/Cart/items/5'),
            { quantity: 3 }
        );
    });

    test('removeFromCart вызывает DELETE', async () => {
        localStorage.setItem('token', 'fake-token');
        localStorage.setItem('userId', '1');

        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ Items: [], TotalQuantity: 0 })
        });

        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => expect(result.current.loading).toBe(false));

        http.delete.mockResolvedValueOnce({ ok: true });
        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ Items: [], TotalQuantity: 0 })
        });

        await act(async () => {
            await result.current.removeFromCart(5);
        });

        expect(http.delete).toHaveBeenCalledWith(
            expect.stringContaining('/api/Cart/items/5')
        );
    });

    test('clearCart возвращает success: false, если не авторизован', async () => {
        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => expect(result.current.loading).toBe(false));

        let response;
        await act(async () => {
            response = await result.current.clearCart();
        });

        expect(response.success).toBe(false);
    });

    test('clearCart вызывает DELETE при авторизации', async () => {
        localStorage.setItem('token', 'fake-token');
        localStorage.setItem('userId', '1');

        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ Items: [], TotalQuantity: 0 })
        });

        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => expect(result.current.loading).toBe(false));

        http.delete.mockResolvedValueOnce({ ok: true });
        http.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ Items: [], TotalQuantity: 0 })
        });

        await act(async () => {
            await result.current.clearCart();
        });

        expect(http.delete).toHaveBeenCalledWith(
            expect.stringContaining('/api/Cart')
        );
    });

    test('ошибка загрузки корзины — корзина очищается', async () => {
        localStorage.setItem('token', 'fake-token');
        localStorage.setItem('userId', '1');

        const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});

        http.mockRejectedValueOnce(new Error('Server error'));

        const { result } = renderHook(() => useCart(), { wrapper });

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.cartItems).toEqual([]);
        expect(result.current.cartCount).toBe(0);

        consoleError.mockRestore();
    });
});