import { renderHook, waitFor, act } from '@testing-library/react';
import { useFetchProducts } from './useFetchProducts';

global.fetch = jest.fn();

describe('useFetchProducts', () => {
    beforeEach(() => {
        fetch.mockClear();
        localStorage.clear();
    });

    test('начальное состояние — loading=true, products=[]', () => {
        fetch.mockImplementation(() => new Promise(() => {})); 

        const { result } = renderHook(() =>
            useFetchProducts('/api/test', 'test_cache', 'test_etag')
        );

        expect(result.current.loading).toBe(true);
        expect(result.current.products).toEqual([]);
        expect(result.current.error).toBeNull();
    });

    test('загружает продукты с сервера', async () => {
        const mockProducts = [
            { id: 1, name: 'Товар 1' },
            { id: 2, name: 'Товар 2' }
        ];

        fetch.mockResolvedValueOnce({
            ok: true,
            status: 200,
            headers: new Map([['ETag', 'etag-123']]),
            json: async () => mockProducts
        });

        const { result } = renderHook(() =>
            useFetchProducts('/api/test', 'test_cache', 'test_etag')
        );

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.products).toEqual(mockProducts);
        expect(result.current.error).toBeNull();
    });

    test('использует кэш если есть свежие данные', async () => {
        const cachedProducts = [{ id: 1, name: 'Из кэша' }];
        localStorage.setItem('test_cache', JSON.stringify({
            data: cachedProducts,
            timestamp: Date.now()
        }));

        const { result } = renderHook(() =>
            useFetchProducts('/api/test', 'test_cache', 'test_etag')
        );

        await waitFor(() => {
            expect(result.current.products).toEqual(cachedProducts);
        });

        expect(fetch).not.toHaveBeenCalled();
    });

    test('обрабатывает ошибку 500 без кэша', async () => {
    // Кэша нет
    fetch.mockRejectedValueOnce(new Error('HTTP 500'));

    const { result } = renderHook(() =>
        useFetchProducts('/api/test', 'test_cache', 'test_etag')
    );

    await waitFor(() => {
        expect(result.current.loading).toBe(false);
    });

    // Без кэша показывается оригинальная ошибка
    expect(result.current.error).toBe('HTTP 500');
});

    test('обрабатывает ошибку 500 и падает обратно на свежий кэш', async () => {
        const cachedProducts = [{ id: 1, name: 'Кэш' }];
        localStorage.setItem('test_cache', JSON.stringify({
            data: cachedProducts,
            timestamp: Date.now() - 60 * 1000
        }));

        fetch.mockRejectedValueOnce(new Error('HTTP 500'));

        const { result, rerender } = renderHook(() =>
            useFetchProducts('/api/test', 'test_cache', 'test_etag')
        );

        // Сначала — из кэша (fetch не вызывается)
        await waitFor(() => {
            expect(result.current.products).toEqual(cachedProducts);
        });

        expect(fetch).not.toHaveBeenCalled();
    });

    test('обрабатывает пустой ответ', async () => {
        fetch.mockResolvedValueOnce({
            ok: true,
            status: 200,
            headers: new Map(),
            json: async () => []
        });

        const { result } = renderHook(() =>
            useFetchProducts('/api/test', 'test_cache', 'test_etag')
        );

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.products).toEqual([]);
    });

    test('обрабатывает 304 Not Modified', async () => {
        fetch.mockResolvedValueOnce({
            ok: false,
            status: 304,
            headers: new Map(),
            json: async () => ({})
        });

        const { result } = renderHook(() =>
            useFetchProducts('/api/test', 'test_cache', 'test_etag')
        );

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.error).toBeNull();
    });

    test('handleClearCache очищает кэш и перезагружает', async () => {
        const mockProducts = [{ id: 1 }];

        fetch.mockResolvedValue({
            ok: true,
            status: 200,
            headers: new Map(),
            json: async () => mockProducts
        });

        const { result } = renderHook(() =>
            useFetchProducts('/api/test', 'test_cache', 'test_etag')
        );

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        localStorage.setItem('test_cache', 'some-data');

        act(() => {
            result.current.handleClearCache();
        });

        expect(localStorage.getItem('test_cache')).toBeNull();
    });
});