import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';
import { userStorage } from 'entities/user/lib/userStorage';

export const WishlistContext = createContext(null);

// Без провайдера (например, в изолированных тестах компонентов) избранное просто неактивно
const NOOP_WISHLIST = {
    ids: new Set(),
    count: 0,
    isAuthenticated: false,
    isInWishlist: () => false,
    toggle: async () => ({ success: false }),
    refresh: async () => {}
};

export const useWishlist = () => useContext(WishlistContext) || NOOP_WISHLIST;

/**
 * Избранное пользователя. Хранит только Id товаров (GET /api/Wishlist/ids) —
 * этого достаточно для «сердечек» в каталоге; полный список грузит страница избранного.
 */
export const WishlistProvider = ({ children, authKey }) => {
    const [ids, setIds] = useState(() => new Set());
    const isAuthenticated = userStorage.isAuthenticated();

    const refresh = useCallback(async () => {
        if (!userStorage.isAuthenticated()) {
            setIds(new Set());
            return;
        }
        try {
            const response = await http(Configuration.Wishlist.Ids);
            const data = await response.json();
            setIds(new Set(Array.isArray(data) ? data : []));
        } catch (error) {
            console.error('Ошибка загрузки избранного:', error);
        }
    }, []);

    // authKey меняется при входе/выходе (см. app/providers) — перезагружаем избранное
    useEffect(() => {
        refresh();
    }, [refresh, isAuthenticated, authKey]);

    /**
     * Оптимистичное переключение: сердечко меняется сразу, при ошибке сервера — откатывается.
     */
    const toggle = useCallback(async (productId) => {
        if (!userStorage.isAuthenticated()) {
            return { success: false, message: 'Войдите, чтобы добавлять товары в избранное' };
        }

        const wasInWishlist = ids.has(productId);
        setIds(prev => {
            const next = new Set(prev);
            if (wasInWishlist) next.delete(productId); else next.add(productId);
            return next;
        });

        try {
            const url = `${Configuration.Wishlist.Item}/${productId}`;
            if (wasInWishlist) await http.delete(url);
            else await http.put(url);
            return { success: true, added: !wasInWishlist };
        } catch (error) {
            setIds(prev => {
                const next = new Set(prev);
                if (wasInWishlist) next.add(productId); else next.delete(productId);
                return next;
            });
            return { success: false, message: error.message };
        }
    }, [ids]);

    const value = useMemo(() => ({
        ids,
        count: ids.size,
        isAuthenticated,
        isInWishlist: (productId) => ids.has(productId),
        toggle,
        refresh
    }), [ids, isAuthenticated, toggle, refresh]);

    return (
        <WishlistContext.Provider value={value}>
            {children}
        </WishlistContext.Provider>
    );
};

export default WishlistProvider;
