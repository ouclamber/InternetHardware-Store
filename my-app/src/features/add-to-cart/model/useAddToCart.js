import { useContext } from 'react';
import { CartContext } from 'entities/cart/model/CartContext';
import { useNotification } from 'shared/lib/hooks/useNotification';

export const useAddToCart = () => {
    const context = useContext(CartContext);
    const { notification, showNotification } = useNotification();

    const addToCart = async (productId, quantity = 1, event, productName) => {
        if (event) event.stopPropagation();
        const result = await context.addToCart(productId, quantity);
        if (result.success) {
            showNotification('Товар добавлен в корзину!', 'success', productName || 'Товар');
            return { success: true };
        } else {
            alert('Ошибка при добавлении в корзину');
            return { success: false };
        }
    };

    return { addToCart, notification };
};