import { renderHook, waitFor, act } from '@testing-library/react';
import { useCategoryProducts, buildCatalogQuery } from './useCategoryProducts';

const mockHttp = jest.fn();
jest.mock('shared/api/httpClient', () => ({
    http: (...args) => mockHttp(...args)
}));

let mockCategoriesState;
jest.mock('entities/category/model/useCategories', () => ({
    useCategories: () => mockCategoriesState
}));

const findByName = (name) => (name === 'Смартфоны' ? { Id: 3, Name: 'Смартфоны' } : null);

const pageResponse = (items, totalCount = items.length, totalPages = 1) => ({
    ok: true,
    json: async () => ({ Items: items, TotalCount: totalCount, TotalPages: totalPages, Page: 1, PageSize: 12 })
});

describe('buildCatalogQuery', () => {
    test('пропускает пустые параметры и сортировку по умолчанию', () => {
        expect(buildCatalogQuery({ categoryId: 3, sort: 'relevance', minPrice: '', maxPrice: '', inStockOnly: false, page: 1, pageSize: 6 }))
            .toBe('categoryId=3&page=1&pageSize=6');
    });

    test('передаёт все заданные фильтры', () => {
        const query = new URLSearchParams(buildCatalogQuery({
            categoryId: 3, sort: 'price_desc', minPrice: '1000', maxPrice: 0, inStockOnly: true, page: 2, pageSize: 12
        }));

        expect(query.get('sort')).toBe('price_desc');
        expect(query.get('minPrice')).toBe('1000');
        expect(query.get('maxPrice')).toBe('0');
        expect(query.get('inStockOnly')).toBe('true');
        expect(query.get('page')).toBe('2');
    });

    test('отрицательную цену не отправляет', () => {
        expect(buildCatalogQuery({ categoryId: 1, minPrice: '-5', page: 1, pageSize: 6 })).not.toContain('minPrice');
    });
});

describe('useCategoryProducts', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockCategoriesState = { loading: false, error: null, findByName };
    });

    test('загружает страницу товаров категории с сервера', async () => {
        mockHttp.mockResolvedValueOnce(pageResponse([{ Id: 1, Name: 'iPhone' }], 25, 3));

        const { result } = renderHook(() => useCategoryProducts('Смартфоны', { sort: 'price_asc', page: 2, pageSize: 12 }));

        await waitFor(() => expect(result.current.loading).toBe(false));

        expect(result.current.products).toEqual([{ Id: 1, Name: 'iPhone' }]);
        expect(result.current.totalCount).toBe(25);
        expect(result.current.totalPages).toBe(3);

        const [url, options] = mockHttp.mock.calls[0];
        expect(url).toBe('/api/Products/paged?categoryId=3&sort=price_asc&page=2&pageSize=12');
        expect(options.signal).toBeDefined();
    });

    test('ждёт загрузки категорий', () => {
        mockCategoriesState = { loading: true, error: null, findByName };

        renderHook(() => useCategoryProducts('Смартфоны'));

        expect(mockHttp).not.toHaveBeenCalled();
    });

    test('неизвестная категория — ошибка без запроса', async () => {
        const { result } = renderHook(() => useCategoryProducts('Пылесосы'));

        await waitFor(() => expect(result.current.loading).toBe(false));
        expect(result.current.error).toContain('Пылесосы');
        expect(mockHttp).not.toHaveBeenCalled();
    });

    test('ошибка загрузки категорий пробрасывается', async () => {
        mockCategoriesState = { loading: false, error: 'Нет связи', findByName };

        const { result } = renderHook(() => useCategoryProducts('Смартфоны'));

        await waitFor(() => expect(result.current.error).toBe('Нет связи'));
    });

    test('ошибка сервера показывается текстом', async () => {
        mockHttp.mockRejectedValueOnce(new Error('Минимальная цена больше максимальной'));

        const { result } = renderHook(() => useCategoryProducts('Смартфоны'));

        await waitFor(() => expect(result.current.error).toBe('Минимальная цена больше максимальной'));
    });

    test('смена параметров — новый запрос, предыдущий отменяется', async () => {
        mockHttp.mockResolvedValue(pageResponse([]));

        const { result, rerender } = renderHook(
            ({ page }) => useCategoryProducts('Смартфоны', { page }),
            { initialProps: { page: 1 } }
        );
        await waitFor(() => expect(result.current.loading).toBe(false));

        const firstSignal = mockHttp.mock.calls[0][1].signal;
        rerender({ page: 2 });
        await waitFor(() => expect(mockHttp).toHaveBeenCalledTimes(2));

        expect(firstSignal.aborted).toBe(true);
        expect(mockHttp.mock.calls[1][0]).toContain('page=2');
    });

    test('reload повторяет запрос', async () => {
        mockHttp.mockResolvedValue(pageResponse([]));

        const { result } = renderHook(() => useCategoryProducts('Смартфоны'));
        await waitFor(() => expect(result.current.loading).toBe(false));

        act(() => result.current.reload());

        await waitFor(() => expect(mockHttp).toHaveBeenCalledTimes(2));
    });

    test('AbortError не считается ошибкой', async () => {
        const abortError = new Error('aborted');
        abortError.name = 'AbortError';
        mockHttp.mockRejectedValueOnce(abortError);

        const { result } = renderHook(() => useCategoryProducts('Смартфоны'));

        await new Promise(r => setTimeout(r, 0));
        expect(result.current.error).toBeNull();
    });
});
