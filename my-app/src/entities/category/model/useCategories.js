import { useState, useEffect, useCallback, useRef } from 'react';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';

// Глобальный кэш — на всё приложение
let categoriesCache = null;
let categoriesCacheTime = null;
const CACHE_TTL = 5 * 60 * 1000; // 5 минут

export const useCategories = () => {
    const [categories, setCategories] = useState(categoriesCache || []);
    const [loading, setLoading] = useState(!categoriesCache);
    const [error, setError] = useState(null);
    const abortRef = useRef(null);

    const load = useCallback(async () => {
        // Если свежий кэш — не грузим
        if (categoriesCache && categoriesCacheTime &&
            (Date.now() - categoriesCacheTime) < CACHE_TTL) {
            setCategories(categoriesCache);
            setLoading(false);
            return;
        }

        if (abortRef.current) abortRef.current.abort();
        abortRef.current = new AbortController();

        try {
            setLoading(true);
            setError(null);

            const response = await http(Configuration.Categories.GetAll, {
                signal: abortRef.current.signal
            });
            const data = await response.json();

            categoriesCache = data;
            categoriesCacheTime = Date.now();

            setCategories(data);
        } catch (err) {
            if (err.name === 'AbortError') return;
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        load();
        return () => {
            if (abortRef.current) abortRef.current.abort();
        };
    }, [load]);

    const findByName = useCallback((name) => {
        if (!categories || !name) return null;
        const lower = name.toLowerCase();
        return categories.find(c =>
            (c.name || c.Name || '').toLowerCase().includes(lower)
        );
    }, [categories]);

    return { categories, loading, error, reload: load, findByName };
};