import { useCallback, useEffect, useRef, useState } from 'react';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';
import { useCategories } from 'entities/category/model/useCategories';

const pick = (obj, ...keys) => {
    if (!obj) return undefined;
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

/**
 * Собирает query-string для GET /api/Products/paged. Пустые параметры не передаются.
 */
export const buildCatalogQuery = ({ categoryId, sort, minPrice, maxPrice, inStockOnly, page, pageSize }) => {
    const params = new URLSearchParams();
    if (categoryId) params.set('categoryId', categoryId);
    if (sort && sort !== 'relevance') params.set('sort', sort);
    if (minPrice !== '' && minPrice !== null && minPrice !== undefined && Number(minPrice) >= 0) params.set('minPrice', minPrice);
    if (maxPrice !== '' && maxPrice !== null && maxPrice !== undefined && Number(maxPrice) >= 0) params.set('maxPrice', maxPrice);
    if (inStockOnly) params.set('inStockOnly', 'true');
    params.set('page', page || 1);
    params.set('pageSize', pageSize || 12);
    return params.toString();
};

/**
 * Товары категории с фильтрами, сортировкой и пагинацией НА СЕРВЕРЕ.
 *
 * Раньше страница скачивала всю категорию и фильтровала/листала её в браузере,
 * а в localStorage кэшировались только первые 5 товаров — при возврате на страницу
 * пользователь на мгновение видел обрезанный список.
 */
export const useCategoryProducts = (categoryName, {
    sort = 'relevance',
    minPrice = '',
    maxPrice = '',
    inStockOnly = false,
    page = 1,
    pageSize = 12
} = {}) => {
    const [products, setProducts] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [totalPages, setTotalPages] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [reloadKey, setReloadKey] = useState(0);
    const abortRef = useRef(null);

    const { loading: categoriesLoading, error: categoriesError, findByName } = useCategories();

    useEffect(() => {
        if (categoriesLoading) return undefined;

        if (categoriesError) {
            setError(categoriesError);
            setLoading(false);
            return undefined;
        }

        const category = findByName(categoryName);
        if (!category) {
            setError(`Категория "${categoryName}" не найдена`);
            setLoading(false);
            return undefined;
        }

        if (abortRef.current) abortRef.current.abort();
        const controller = new AbortController();
        abortRef.current = controller;

        const query = buildCatalogQuery({
            categoryId: pick(category, 'id', 'Id'),
            sort, minPrice, maxPrice, inStockOnly, page, pageSize
        });

        setLoading(true);
        setError(null);

        (async () => {
            try {
                const response = await http(`${Configuration.Products.Paged}?${query}`, { signal: controller.signal });
                const data = await response.json();

                setProducts(pick(data, 'Items', 'items') || []);
                setTotalCount(pick(data, 'TotalCount', 'totalCount') || 0);
                setTotalPages(pick(data, 'TotalPages', 'totalPages') || 0);
                setLoading(false);
            } catch (err) {
                if (err.name === 'AbortError') return;
                setError(err.message || 'Не удалось загрузить товары');
                setLoading(false);
            }
        })();

        return () => controller.abort();
    }, [categoryName, categoriesLoading, categoriesError, findByName,
        sort, minPrice, maxPrice, inStockOnly, page, pageSize, reloadKey]);

    const reload = useCallback(() => setReloadKey(k => k + 1), []);

    return { products, totalCount, totalPages, loading, error, reload };
};
