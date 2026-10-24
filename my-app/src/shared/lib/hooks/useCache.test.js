import { renderHook, act } from '@testing-library/react';
import { useCache } from './useCache';

describe('useCache', () => {
    beforeEach(() => {
        localStorage.clear();
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test('getFromCache возвращает null если кэша нет', () => {
        const { result } = renderHook(() => useCache('test_cache', 'test_etag'));
        expect(result.current.getFromCache()).toBeNull();
    });

    test('saveToCache сохраняет данные в localStorage', () => {
        const { result } = renderHook(() => useCache('test_cache', 'test_etag'));
        const data = [{ id: 1, name: 'Test' }];

        act(() => {
            result.current.saveToCache(data);
        });

        const cached = JSON.parse(localStorage.getItem('test_cache'));
        expect(cached.data).toEqual(data);
        expect(cached.timestamp).toBeDefined();
    });

    test('getFromCache возвращает сохранённые данные', () => {
        const { result } = renderHook(() => useCache('test_cache', 'test_etag'));
        const data = [{ id: 1, name: 'Test' }];

        act(() => {
            result.current.saveToCache(data);
        });

        const cached = result.current.getFromCache();
        expect(cached).toEqual(data);
    });

    test('getFromCache возвращает null если кэш устарел', () => {
        const { result } = renderHook(() => useCache('test_cache', 'test_etag', 1000));
        const data = [{ id: 1 }];

        act(() => {
            result.current.saveToCache(data);
        });

        act(() => {
            jest.advanceTimersByTime(2000);
        });

        expect(result.current.getFromCache()).toBeNull();
    });

    test('clearCache удаляет данные из localStorage', () => {
        const { result } = renderHook(() => useCache('test_cache', 'test_etag'));

        act(() => {
            result.current.saveToCache([{ id: 1 }]);
            result.current.setEtag('etag-value');
        });

        expect(localStorage.getItem('test_cache')).not.toBeNull();
        expect(localStorage.getItem('test_etag')).toBe('etag-value');

        act(() => {
            result.current.clearCache();
        });

        expect(localStorage.getItem('test_cache')).toBeNull();
        expect(localStorage.getItem('test_etag')).toBeNull();
    });

    test('getEtag возвращает пустую строку если etagKey не задан', () => {
        const { result } = renderHook(() => useCache('test_cache', null));
        expect(result.current.getEtag()).toBe('');
    });

    test('setEtag сохраняет etag', () => {
        const { result } = renderHook(() => useCache('test_cache', 'test_etag'));

        act(() => {
            result.current.setEtag('my-etag');
        });

        expect(result.current.getEtag()).toBe('my-etag');
    });

    test('saveToCache ограничивает массив до 5 элементов', () => {
        const { result } = renderHook(() => useCache('test_cache', 'test_etag'));
        const data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        act(() => {
            result.current.saveToCache(data);
        });

        const cached = JSON.parse(localStorage.getItem('test_cache'));
        expect(cached.data.length).toBe(5);
        expect(cached.data).toEqual([1, 2, 3, 4, 5]);
    });

    test('getFromCache обрабатывает битый JSON', () => {
        localStorage.setItem('test_cache', 'invalid-json-{{{');

        const { result } = renderHook(() => useCache('test_cache', 'test_etag'));
        expect(result.current.getFromCache()).toBeNull();
    });
});