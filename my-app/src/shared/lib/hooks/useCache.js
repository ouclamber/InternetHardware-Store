import { useCallback } from 'react';

export const useCache = (cacheKey, etagKey, expiry = 5 * 60 * 1000) => {
    const getFromCache = useCallback(() => {
        try {
            const cached = localStorage.getItem(cacheKey);
            if (cached) {
                const { data, timestamp } = JSON.parse(cached);
                if (Date.now() - timestamp < expiry) return data;
            }
        } catch (error) {
            console.warn('Ошибка чтения кэша:', error);
        }
        return null;
    }, [cacheKey, expiry]);

    const saveToCache = useCallback((data) => {
        try {
            localStorage.setItem(cacheKey, JSON.stringify({
                data: Array.isArray(data) ? data.slice(0, 5) : data,
                timestamp: Date.now()
            }));
        } catch (error) {
            console.warn('Ошибка сохранения кэша:', error);
        }
    }, [cacheKey]);

    const clearCache = useCallback(() => {
        localStorage.removeItem(cacheKey);
        if (etagKey) localStorage.removeItem(etagKey);
    }, [cacheKey, etagKey]);

    const getEtag = useCallback(
        () => (etagKey ? localStorage.getItem(etagKey) : ''),
        [etagKey]
    );

    const setEtag = useCallback((etag) => {
        if (etagKey && etag) localStorage.setItem(etagKey, etag);
    }, [etagKey]);

    return { getFromCache, saveToCache, clearCache, getEtag, setEtag };
};