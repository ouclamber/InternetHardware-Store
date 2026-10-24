import { renderHook, act } from '@testing-library/react';
import { useNotification } from './useNotification';

describe('useNotification', () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test('начальное состояние — notification скрыто', () => {
        const { result } = renderHook(() => useNotification());
        expect(result.current.notification.show).toBe(false);
        expect(result.current.notification.message).toBe('');
        expect(result.current.notification.type).toBe('success');
    });

    test('showNotification показывает уведомление', () => {
        const { result } = renderHook(() => useNotification());

        act(() => {
            result.current.showNotification('Товар добавлен', 'success', 'Ноутбук');
        });

        expect(result.current.notification.show).toBe(true);
        expect(result.current.notification.message).toBe('Товар добавлен');
        expect(result.current.notification.type).toBe('success');
        expect(result.current.notification.productName).toBe('Ноутбук');
    });

    test('showNotification автоматически скрывает через 3 секунды', () => {
        const { result } = renderHook(() => useNotification());

        act(() => {
            result.current.showNotification('Товар добавлен');
        });

        expect(result.current.notification.show).toBe(true);

        act(() => {
            jest.advanceTimersByTime(3000);
        });

        expect(result.current.notification.show).toBe(false);
    });

    test('showNotification с типом error', () => {
        const { result } = renderHook(() => useNotification());

        act(() => {
            result.current.showNotification('Ошибка', 'error');
        });

        expect(result.current.notification.type).toBe('error');
    });

    test('hideNotification скрывает уведомление вручную', () => {
        const { result } = renderHook(() => useNotification());

        act(() => {
            result.current.showNotification('Тест');
        });

        expect(result.current.notification.show).toBe(true);

        act(() => {
            result.current.hideNotification();
        });

        expect(result.current.notification.show).toBe(false);
        expect(result.current.notification.message).toBe('');
    });

    test('повторный вызов showNotification сбрасывает таймер', () => {
        const { result } = renderHook(() => useNotification());

        act(() => {
            result.current.showNotification('Первое');
        });

        act(() => {
            jest.advanceTimersByTime(2000);
        });

        act(() => {
            result.current.showNotification('Второе');
        });

        act(() => {
            jest.advanceTimersByTime(2000);
        });

        expect(result.current.notification.show).toBe(true);
        expect(result.current.notification.message).toBe('Второе');
    });
});