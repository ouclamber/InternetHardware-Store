import { renderHook, act } from '@testing-library/react';
import { useDebouncedValue } from './useDebouncedValue';

describe('useDebouncedValue', () => {
    beforeEach(() => jest.useFakeTimers());
    afterEach(() => jest.useRealTimers());

    test('сразу возвращает начальное значение', () => {
        const { result } = renderHook(() => useDebouncedValue('a', 300));
        expect(result.current).toBe('a');
    });

    test('обновляется только после паузы', () => {
        const { result, rerender } = renderHook(({ v }) => useDebouncedValue(v, 300), { initialProps: { v: 'a' } });

        rerender({ v: 'ab' });
        act(() => { jest.advanceTimersByTime(299); });
        expect(result.current).toBe('a');

        act(() => { jest.advanceTimersByTime(1); });
        expect(result.current).toBe('ab');
    });

    test('частые изменения схлопываются в одно', () => {
        const { result, rerender } = renderHook(({ v }) => useDebouncedValue(v, 300), { initialProps: { v: '' } });

        rerender({ v: '1' });
        act(() => { jest.advanceTimersByTime(200); });
        rerender({ v: '10' });
        act(() => { jest.advanceTimersByTime(200); });
        rerender({ v: '100' });
        expect(result.current).toBe('');

        act(() => { jest.advanceTimersByTime(300); });
        expect(result.current).toBe('100');
    });
});
