import { useState, useEffect, useCallback, useRef } from 'react';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';
import { useCategories } from 'entities/category/model/useCategories';

export const useFetchProductsByCategory = (categoryName, cacheKey, etagKey) => {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const abortRef = useRef(null);

    const { loading: categoriesLoading, error: categoriesError, findByName } = useCategories();

    const loadFromLocalStorage = useCallback(() => {
        try {
            const cached = localStorage.getItem(cacheKey);
            if (cached) {
                const parsed = JSON.parse(cached);
                if (Date.now() - parsed.timestamp < 5 * 60 * 1000) {
                    return parsed.data;
                }
            }
        } catch (e) {
            console.warn('Ошибка чтения кэша:', e);
        }
        return null;
    }, [cacheKey]);

    const saveToLocalStorage = useCallback((data) => {
        try {
            localStorage.setItem(cacheKey, JSON.stringify({
                data: Array.isArray(data) ? data.slice(0, 5) : data,
                timestamp: Date.now()
            }));
        } catch (e) {
            console.warn('Ошибка сохранения кэша:', e);
        }
    }, [cacheKey]);

    const fetchProducts = useCallback(async () => {
        // Пока категории грузятся — ждём
        if (categoriesLoading) return;

        if (categoriesError) {
            setError(categoriesError);
            setLoading(false);
            return;
        }

        const category = findByName(categoryName);
        if (!category) {
            setError(`Категория "${categoryName}" не найдена`);
            setLoading(false);
            return;
        }

        const categoryId = category.id ?? category.Id;

        // Пробуем кэш
        const cached = loadFromLocalStorage();
        if (cached) {
            setProducts(cached);
            setLoading(false);
        }

        if (abortRef.current) abortRef.current.abort();
        abortRef.current = new AbortController();

        try {
            setLoading(true);
            setError(null);

            const response = await http(`${Configuration.Products.ByCategory}/${categoryId}`, {
                signal: abortRef.current.signal
            });
            const data = await response.json();

            saveToLocalStorage(data);
            setProducts(data);
        } catch (err) {
            if (err.name === 'AbortError') return;
            if (cached) {
                setError('Используются кэшированные данные');
            } else {
                setError(err.message);
            }
        } finally {
            setLoading(false);
        }
    }, [categoryName, categoriesLoading, categoriesError, findByName, loadFromLocalStorage, saveToLocalStorage]);

    useEffect(() => {
        fetchProducts();
        return () => {
            if (abortRef.current) abortRef.current.abort();
        };
    }, [fetchProducts]);

    const handleClearCache = useCallback(() => {
        try {
            localStorage.removeItem(cacheKey);
            if (etagKey) localStorage.removeItem(etagKey);
        } catch (e) {
            console.warn('Ошибка очистки кэша:', e);
        }
        setProducts([]);
        setLoading(true);
        fetchProducts();
    }, [cacheKey, etagKey, fetchProducts]);

    return { products, loading, error, fetchProducts, handleClearCache };
};