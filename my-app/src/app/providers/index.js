import React from 'react';
import { BrowserRouter, useLocation } from 'react-router-dom';
import CartProvider from 'entities/cart/model/CartContext';
import WishlistProvider from 'entities/wishlist/model/WishlistContext';
import { userStorage } from 'entities/user/lib/userStorage';

/**
 * Перерисовывается при каждой навигации и передаёт текущий токен:
 * после входа/выхода избранное загружается заново без перезагрузки страницы.
 */
const AuthAwareWishlistProvider = ({ children }) => {
    useLocation();
    return (
        <WishlistProvider authKey={userStorage.getToken() || ''}>
            {children}
        </WishlistProvider>
    );
};

export const AppProviders = ({ children }) => {
    return (
        <CartProvider>
            <BrowserRouter>
                <AuthAwareWishlistProvider>
                    {children}
                </AuthAwareWishlistProvider>
            </BrowserRouter>
        </CartProvider>
    );
};
