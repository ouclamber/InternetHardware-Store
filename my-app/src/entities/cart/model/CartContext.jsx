import React, { createContext, useState, useEffect, useCallback, useContext } from 'react';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';

export const CartContext = createContext(null);

export const useCart = () => {
    const context = useContext(CartContext);
    if (!context) {
        throw new Error('useCart должен использоваться внутри CartProvider');
    }
    return context;
};

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

export const CartProvider = ({ children }) => {
    const [cartItems, setCartItems] = useState([]);
    const [cartCount, setCartCount] = useState(0);
    const [cartTotal, setCartTotal] = useState(0);
    const [loading, setLoading] = useState(false);

    const isAuthenticated = useCallback(() => {
        const token = localStorage.getItem('token');
        const userId = localStorage.getItem('userId');
        return !!(token && userId);
    }, []);

    const mapItemsFromApi = useCallback((apiItems) => {
        if (!Array.isArray(apiItems)) return [];
        return apiItems.map(item => {
            const id = pick(item, 'Id', 'id');
            const productId = pick(item, 'ProductId', 'productId');
            const quantity = pick(item, 'Quantity', 'quantity') || 1;
            const productName = pick(item, 'ProductName', 'productName') || 'Товар';
            const brandName = pick(item, 'BrandName', 'brandName');
            const mainImage = pick(item, 'MainImage', 'mainImage');
            const unitPrice = pick(item, 'UnitPrice', 'unitPrice') || 0;

            return {
                id,
                productId,
                quantity,
                product: {
                    id: productId,
                    name: productName,
                    price: unitPrice,
                    brand: brandName ? { name: brandName } : null,
                    images: mainImage && mainImage !== 'string'
                        ? [{ imageUrl: mainImage, isMain: true }]
                        : []
                }
            };
        });
    }, []);

    const loadCart = useCallback(async () => {
        if (!isAuthenticated()) {
            setCartItems([]);
            setCartCount(0);
            setCartTotal(0);
            return;
        }

        try {
            setLoading(true);
            const response = await http(Configuration.Cart.Get);
            const data = await response.json();

            const apiItems = pick(data, 'Items', 'items') || [];
            const items = mapItemsFromApi(apiItems);

            setCartItems(items);
            setCartCount(pick(data, 'TotalQuantity', 'totalQuantity') || 0);
            setCartTotal(pick(data, 'TotalAmount', 'totalAmount') || 0);
        } catch (error) {
            console.error('Ошибка загрузки корзины:', error);
            setCartItems([]);
            setCartCount(0);
            setCartTotal(0);
        } finally {
            setLoading(false);
        }
    }, [isAuthenticated, mapItemsFromApi]);

    useEffect(() => {
        loadCart();
    }, [loadCart]);

    const addToCart = useCallback(async (productId, quantity = 1) => {
        if (!isAuthenticated()) {
            return { success: false, message: 'Пожалуйста, войдите в систему' };
        }

        try {
            const response = await http.post(Configuration.Cart.AddItem, {
                productId,
                quantity
            });
            const data = await response.json();

            const isSuccess = pick(data, 'isSuccess', 'IsSuccess', 'success', 'Success');

            if (isSuccess) {
                await loadCart();
                return { success: true, message: pick(data, 'message', 'Message') };
            }
            return {
                success: false,
                message: pick(data, 'message', 'Message') || 'Ошибка добавления'
            };
        } catch (error) {
            console.error('Ошибка добавления в корзину:', error);
            return { success: false, message: error.message };
        }
    }, [isAuthenticated, loadCart]);

    const updateCartItem = useCallback(async (productId, quantity) => {
        try {
            await http.put(`${Configuration.Cart.UpdateItem}/${productId}`, { quantity });
            await loadCart();
        } catch (error) {
            console.error('Ошибка обновления корзины:', error);
        }
    }, [loadCart]);

    const removeFromCart = useCallback(async (productId) => {
        try {
            await http.delete(`${Configuration.Cart.RemoveItem}/${productId}`);
            await loadCart();
        } catch (error) {
            console.error('Ошибка удаления из корзины:', error);
        }
    }, [loadCart]);

    const clearCart = useCallback(async () => {
        if (!isAuthenticated()) return { success: false };

        try {
            await http.delete(Configuration.Cart.Clear);
            await loadCart();
            return { success: true };
        } catch (error) {
            console.error('Ошибка очистки корзины:', error);
            return { success: false };
        }
    }, [isAuthenticated, loadCart]);

    const value = {
        cartItems,
        cartCount,
        cartTotal,
        loading,
        addToCart,
        updateCartItem,
        removeFromCart,
        clearCart,
        updateCartCount: loadCart,
        refreshCart: loadCart
    };

    return (
        <CartContext.Provider value={value}>
            {children}
        </CartContext.Provider>
    );
};

export default CartProvider;