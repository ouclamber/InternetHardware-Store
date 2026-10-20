import { useState, useEffect, useCallback, useRef } from 'react';
import { useCache } from 'shared/lib/hooks/useCache';
import { http } from 'shared/api/httpClient';

export const useFetchProducts = (apiUrl, cacheKey, etagKey) => {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const abortControllerRef = useRef(null);
    const { getFromCache, saveToCache, clearCache, getEtag, setEtag } = useCache(cacheKey, etagKey);

    const fetchProducts = useCallback(async () => {
        if (abortControllerRef.current) abortControllerRef.current.abort();
        abortControllerRef.current = new AbortController();

        try {
            setLoading(true);
            setError(null);

            const response = await http(apiUrl, {
                signal: abortControllerRef.current.signal,
                headers: {
                    'If-None-Match': getEtag() || '',
                    'Cache-Control': 'no-cache'
                }
            });

            if (response.status === 304) {
                setLoading(false);
                return;
            }

            const data = await response.json();
            const etag = response.headers.get('ETag');
            setEtag(etag);

            saveToCache(data);
            setProducts(data);
            setLoading(false);
        } catch (err) {
            if (err.name === 'AbortError') return;
            if (err.status === 304) {
                setLoading(false);
                return;
            }

            const cached = getFromCache();
            if (cached) {
                setProducts(cached);
                setError('Используются кэшированные данные');
            } else {
                setError(err.message);
            }
            setLoading(false);
        }
    }, [apiUrl, getEtag, setEtag, saveToCache, getFromCache]);

    useEffect(() => {
        const cached = getFromCache();
        if (cached) {
            setProducts(cached);
            setLoading(false);
        } else {
            fetchProducts();
        }

        return () => {
            if (abortControllerRef.current) abortControllerRef.current.abort();
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [apiUrl]);

    const handleClearCache = useCallback(() => {
        clearCache();
        setProducts([]);
        setLoading(true);
        fetchProducts();
    }, [clearCache, fetchProducts]);

    return { products, loading, error, fetchProducts, handleClearCache };
};