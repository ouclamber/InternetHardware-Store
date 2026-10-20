import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import CartProvider from 'entities/cart/model/CartContext';

export const AppProviders = ({ children }) => {
    return (
        <CartProvider>
            <BrowserRouter>
                {children}
            </BrowserRouter>
        </CartProvider>
    );
};