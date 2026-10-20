import { renderHook, waitFor } from '@testing-library/react';
import { useFetchProductsByCategory } from './useFetchProductsByCategory';

const mockHttp = jest.fn();

jest.mock('shared/api/httpClient', () => ({
    http: (...args) => mockHttp(...args)
}));

jest.mock('entities/category/model/useCategories', () => ({
    useCategories: () => ({
        categories: [{ Id: 1, Name: 'Ноутбуки' }],
        loading: false,
        error: null,
        findByName: (name) => name === 'Ноутбуки' ? { Id: 1, Name: 'Ноутбуки' } : null
    })
}));

const mockProducts = [
    { Id: 1, Name: 'ASUS ROG', Price: 129999 },
    { Id: 2, Name: 'MacBook Air', Price: 149999 }
];

describe('useFetchProductsByCategory', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
    });

    test('загружает товары', async () => {
        mockHttp.mockResolvedValueOnce({
            ok: true,
            json: async () => mockProducts
        });

        const { result } = renderHook(() =>
            useFetchProductsByCategory('Ноутбуки', 'test_cache', 'test_etag')
        );

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.products).toEqual(mockProducts);
    });

    test('возвращает ошибку, если категория не найдена', async () => {
        const { result } = renderHook(() =>
            useFetchProductsByCategory('Несуществующая', 'test_cache', 'test_etag')
        );

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.error).toContain('не найдена');
    });

    test('обрабатывает ошибку API', async () => {
        const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
        mockHttp.mockRejectedValue(new Error('Network error'));

        const { result } = renderHook(() =>
            useFetchProductsByCategory('Ноутбуки', 'test_cache', 'test_etag')
        );

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.products).toEqual([]);
        consoleError.mockRestore();
    });

    test('возвращает объект с нужными полями', () => {
        mockHttp.mockResolvedValueOnce({
            ok: true,
            json: async () => []
        });

        const { result } = renderHook(() =>
            useFetchProductsByCategory('Ноутбуки', 'test_cache', 'test_etag')
        );

        expect(result.current).toHaveProperty('products');
        expect(result.current).toHaveProperty('loading');
        expect(result.current).toHaveProperty('error');
        expect(result.current).toHaveProperty('fetchProducts');
        expect(result.current).toHaveProperty('handleClearCache');
    });
});