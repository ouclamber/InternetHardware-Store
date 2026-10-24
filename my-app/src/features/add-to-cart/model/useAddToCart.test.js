import { renderHook, act, waitFor } from '@testing-library/react';
import { useAddToCart } from './useAddToCart';

// Мок CartContext
const mockAddToCart = jest.fn();
jest.mock('entities/cart/model/CartContext', () => ({
    CartContext: {}
}));

// Мок useContext
jest.mock('react', () => ({
    ...jest.requireActual('react'),
    useContext: () => ({
        addToCart: mockAddToCart
    })
}));

describe('useAddToCart', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test('изначально notification.show = false', () => {
        const { result } = renderHook(() => useAddToCart());
        expect(result.current.notification.show).toBe(false);
    });

    test('addToCart вызывает context.addToCart', async () => {
        mockAddToCart.mockResolvedValue({ success: true });
        const { result } = renderHook(() => useAddToCart());

        await act(async () => {
            await result.current.addToCart(1, 2, null, 'Товар');
        });

        expect(mockAddToCart).toHaveBeenCalledWith(1, 2);
    });

    test('показывает notification при успехе', async () => {
        mockAddToCart.mockResolvedValue({ success: true });
        const { result } = renderHook(() => useAddToCart());

        await act(async () => {
            await result.current.addToCart(1, 1, null, 'ASUS ROG');
        });

        expect(result.current.notification.show).toBe(true);
        expect(result.current.notification.productName).toBe('ASUS ROG');
    });

    test('при ошибке показывает уведомление с текстом сервера (без alert)', async () => {
        const alertSpy = jest.spyOn(window, 'alert').mockImplementation(() => {});
        mockAddToCart.mockResolvedValue({ success: false, message: 'Недостаточно товара на складе: доступно 1 шт.' });
        const { result } = renderHook(() => useAddToCart());

        let response;
        await act(async () => {
            response = await result.current.addToCart(1, 1, null, 'Товар');
        });

        expect(response).toEqual({ success: false, message: 'Недостаточно товара на складе: доступно 1 шт.' });
        expect(result.current.notification.show).toBe(true);
        expect(result.current.notification.type).toBe('error');
        expect(result.current.notification.message).toBe('Недостаточно товара на складе: доступно 1 шт.');
        expect(alertSpy).not.toHaveBeenCalled();
        alertSpy.mockRestore();
    });

    test('при ошибке без текста — сообщение по умолчанию', async () => {
        mockAddToCart.mockResolvedValue({ success: false });
        const { result } = renderHook(() => useAddToCart());

        await act(async () => {
            await result.current.addToCart(1, 1, null, 'Товар');
        });

        expect(result.current.notification.message).toBe('Не удалось добавить товар в корзину');
    });

    test('notification скрывается через 3 секунды', async () => {
        mockAddToCart.mockResolvedValue({ success: true });
        const { result } = renderHook(() => useAddToCart());

        await act(async () => {
            await result.current.addToCart(1, 1, null, 'Товар');
        });

        expect(result.current.notification.show).toBe(true);

        act(() => {
            jest.advanceTimersByTime(3000);
        });

        expect(result.current.notification.show).toBe(false);
    });
});