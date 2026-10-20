import { renderHook, waitFor } from '@testing-library/react';
import { useCategories } from './useCategories';

const mockHttp = jest.fn();

jest.mock('shared/api/httpClient', () => ({
    http: (...args) => mockHttp(...args)
}));

const mockCategories = [
    { Id: 1, Name: 'Ноутбуки' },
    { Id: 2, Name: 'Компьютеры' },
    { Id: 3, Name: 'Смартфоны' }
];

describe('useCategories', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
    });

    test('загружает категории с API', async () => {
        mockHttp.mockResolvedValueOnce({
            ok: true,
            json: async () => mockCategories
        });

        const { result } = renderHook(() => useCategories());

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.categories.length).toBeGreaterThan(0);
    });

    test('findByName находит категорию по имени', async () => {
        mockHttp.mockResolvedValueOnce({
            ok: true,
            json: async () => mockCategories
        });

        const { result } = renderHook(() => useCategories());

        await waitFor(() => expect(result.current.loading).toBe(false));

        const found = result.current.findByName('Ноутбуки');
        expect(found).toBeTruthy();
        expect(found.Id).toBe(1);
    });

    test('findByName возвращает falsy, если категория не найдена', async () => {
        mockHttp.mockResolvedValueOnce({
            ok: true,
            json: async () => mockCategories
        });

        const { result } = renderHook(() => useCategories());

        await waitFor(() => expect(result.current.loading).toBe(false));

        expect(result.current.findByName('Не существует')).toBeFalsy();
    });

    test('возвращает объект с нужными полями', () => {
        mockHttp.mockResolvedValueOnce({
            ok: true,
            json: async () => []
        });

        const { result } = renderHook(() => useCategories());

        expect(result.current).toHaveProperty('categories');
        expect(result.current).toHaveProperty('loading');
        expect(result.current).toHaveProperty('error');
        expect(result.current).toHaveProperty('findByName');
        expect(result.current).toHaveProperty('reload');
    });
});